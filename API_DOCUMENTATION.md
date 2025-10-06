# Car Dealership API - توثيق شامل لنقاط النهاية

## جدول المحتويات

1. [المصادقة (Authentication)](#المصادقة-authentication)
2. [المركبات (Vehicles)](#المركبات-vehicles)
3. [المشتريات (Purchases)](#المشتريات-purchases)
4. [المستخدمين (Users)](#المستخدمين-users)
5. [رموز الحالة (Status Codes)](#رموز-الحالة-status-codes)

---

## المصادقة (Authentication)

### 1. طلب التسجيل

**Endpoint:** `POST /api/Auth/register/request`

**الوصف:** الخطوة الأولى لتسجيل مستخدم جديد. يولد رمز OTP ويرسله.

**الصلاحيات:** عام (لا يتطلب مصادقة)

**Request Body:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "role": "Customer" // أو "Admin"
}
```

**Response (200 OK):**
```json
{
  "message": "OTP sent successfully. Please verify to complete registration.",
  "email": "user@example.com"
}
```

**Response (400 Bad Request):**
```json
{
  "message": "User with this email already exists"
}
```

---

### 2. التحقق من التسجيل

**Endpoint:** `POST /api/Auth/register/verify?otpCode={code}`

**الوصف:** الخطوة الثانية لإكمال التسجيل بعد التحقق من OTP.

**الصلاحيات:** عام (لا يتطلب مصادقة)

**Query Parameters:**
- `otpCode` (string, required): رمز OTP المستلم

**Request Body:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "role": "Customer"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "role": "Customer",
  "message": "Registration successful"
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Invalid or expired OTP"
}
```

---

### 3. طلب تسجيل الدخول

**Endpoint:** `POST /api/Auth/login/request`

**الوصف:** الخطوة الأولى لتسجيل الدخول. يتحقق من بيانات الاعتماد ويولد OTP.

**الصلاحيات:** عام (لا يتطلب مصادقة)

**Request Body:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response (200 OK):**
```json
{
  "message": "Credentials verified. OTP sent for final authentication.",
  "email": "user@example.com"
}
```

**Response (401 Unauthorized):**
```json
{
  "message": "Invalid email or password"
}
```

---

### 4. التحقق من تسجيل الدخول

**Endpoint:** `POST /api/Auth/login/verify`

**الوصف:** الخطوة الثانية لإكمال تسجيل الدخول بعد التحقق من OTP.

**الصلاحيات:** عام (لا يتطلب مصادقة)

**Request Body:**
```json
{
  "email": "string",
  "otpCode": "string"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "role": "Admin",
  "message": "Login successful"
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Invalid or expired OTP"
}
```

---

## المركبات (Vehicles)

### 1. تصفح المركبات

**Endpoint:** `GET /api/Vehicles`

**الوصف:** عرض جميع المركبات مع إمكانية الفلترة.

**الصلاحيات:** مصادقة مطلوبة (Customer أو Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Query Parameters (اختيارية):**
- `make` (string): اسم الشركة المصنعة
- `model` (string): موديل المركبة
- `minYear` (integer): الحد الأدنى للسنة
- `maxYear` (integer): الحد الأقصى للسنة
- `minPrice` (decimal): الحد الأدنى للسعر
- `maxPrice` (decimal): الحد الأقصى للسعر
- `isAvailable` (boolean): حالة التوفر

**مثال:**
```
GET /api/Vehicles?make=Toyota&minPrice=20000&maxPrice=50000&isAvailable=true
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "make": "Toyota",
    "model": "Camry",
    "year": 2023,
    "price": 28500.00,
    "color": "Silver",
    "mileage": 15000,
    "vin": "1HGBH41JXMN109186",
    "isAvailable": true,
    "description": "Excellent condition"
  }
]
```

---

### 2. عرض تفاصيل مركبة

**Endpoint:** `GET /api/Vehicles/{id}`

**الوصف:** الحصول على تفاصيل مركبة محددة.

**الصلاحيات:** مصادقة مطلوبة (Customer أو Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer, required): معرف المركبة

**Response (200 OK):**
```json
{
  "id": 1,
  "make": "Toyota",
  "model": "Camry",
  "year": 2023,
  "price": 28500.00,
  "color": "Silver",
  "mileage": 15000,
  "vin": "1HGBH41JXMN109186",
  "isAvailable": true,
  "description": "Excellent condition, one owner, full service history"
}
```

**Response (404 Not Found):**
```json
{
  "message": "Vehicle not found"
}
```

---

### 3. إضافة مركبة جديدة

**Endpoint:** `POST /api/Vehicles`

**الوصف:** إضافة مركبة جديدة للمخزون.

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Request Body:**
```json
{
  "make": "Toyota",
  "model": "Camry",
  "year": 2024,
  "price": 35000.00,
  "color": "Silver",
  "mileage": 0,
  "vin": "1HGBH41JXMN109999",
  "description": "Brand new Toyota Camry"
}
```

**Response (201 Created):**
```json
{
  "id": 11,
  "make": "Toyota",
  "model": "Camry",
  "year": 2024,
  "price": 35000.00,
  "color": "Silver",
  "mileage": 0,
  "vin": "1HGBH41JXMN109999",
  "isAvailable": true,
  "description": "Brand new Toyota Camry"
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Vehicle with this VIN already exists"
}
```

---

### 4. طلب تحديث مركبة

**Endpoint:** `POST /api/Vehicles/update/request`

**الوصف:** الخطوة الأولى لتحديث مركبة. يولد OTP.

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Request Body:**
```json
{
  "vehicleId": 1,
  "make": "Toyota",
  "model": "Camry",
  "year": 2023,
  "price": 27000.00,
  "color": "Silver",
  "mileage": 16000,
  "isAvailable": true,
  "description": "Updated description"
}
```

**Response (200 OK):**
```json
{
  "message": "OTP sent successfully. Please verify to complete vehicle update.",
  "email": "admin@cardealership.com"
}
```

---

### 5. التحقق من تحديث مركبة

**Endpoint:** `PUT /api/Vehicles/update/verify?otpCode={code}`

**الوصف:** الخطوة الثانية لإكمال تحديث المركبة بعد التحقق من OTP.

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Query Parameters:**
- `otpCode` (string, required): رمز OTP المستلم

**Request Body:**
```json
{
  "vehicleId": 1,
  "price": 27000.00,
  "mileage": 16000
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "make": "Toyota",
  "model": "Camry",
  "year": 2023,
  "price": 27000.00,
  "color": "Silver",
  "mileage": 16000,
  "vin": "1HGBH41JXMN109186",
  "isAvailable": true,
  "description": "Updated description"
}
```

---

### 6. حذف مركبة

**Endpoint:** `DELETE /api/Vehicles/{id}`

**الوصف:** حذف مركبة من المخزون.

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Path Parameters:**
- `id` (integer, required): معرف المركبة

**Response (200 OK):**
```json
{
  "message": "Vehicle deleted successfully"
}
```

---

## المشتريات (Purchases)

### 1. طلب شراء مركبة

**Endpoint:** `POST /api/Purchases/request`

**الوصف:** الخطوة الأولى لطلب شراء مركبة. يولد OTP.

**الصلاحيات:** Customer فقط

**Headers:**
```
Authorization: Bearer {customer_token}
```

**Request Body:**
```json
{
  "vehicleId": 1
}
```

**Response (200 OK):**
```json
{
  "message": "OTP sent successfully. Please verify to complete purchase request.",
  "email": "customer@example.com"
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Vehicle is not available for purchase"
}
```

---

### 2. التحقق من طلب الشراء

**Endpoint:** `POST /api/Purchases/verify?otpCode={code}`

**الوصف:** الخطوة الثانية لإكمال طلب الشراء بعد التحقق من OTP.

**الصلاحيات:** Customer فقط

**Headers:**
```
Authorization: Bearer {customer_token}
```

**Query Parameters:**
- `otpCode` (string, required): رمز OTP المستلم

**Request Body:**
```json
{
  "vehicleId": 1
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "vehicleId": 1,
  "vehicleMake": "Toyota",
  "vehicleModel": "Camry",
  "vehicleYear": 2023,
  "purchasePrice": 28500.00,
  "purchaseDate": "2025-10-06T13:02:01.3521992Z",
  "status": "Pending"
}
```

---

### 3. عرض سجل المشتريات الخاص

**Endpoint:** `GET /api/Purchases/my-purchases`

**الوصف:** عرض سجل المشتريات للعميل الحالي.

**الصلاحيات:** Customer فقط

**Headers:**
```
Authorization: Bearer {customer_token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "vehicleId": 1,
    "vehicleMake": "Toyota",
    "vehicleModel": "Camry",
    "vehicleYear": 2023,
    "purchasePrice": 28500.00,
    "purchaseDate": "2025-10-06T13:02:01.3521992Z",
    "status": "Pending"
  }
]
```

---

### 4. عرض تفاصيل مشترى محدد

**Endpoint:** `GET /api/Purchases/{id}`

**الوصف:** الحصول على تفاصيل مشترى محدد.

**الصلاحيات:** 
- Customer: يمكنه رؤية مشترياته فقط
- Admin: يمكنه رؤية جميع المشتريات

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer, required): معرف المشترى

**Response (200 OK):**
```json
{
  "id": 1,
  "vehicleId": 1,
  "vehicleMake": "Toyota",
  "vehicleModel": "Camry",
  "vehicleYear": 2023,
  "purchasePrice": 28500.00,
  "purchaseDate": "2025-10-06T13:02:01.3521992Z",
  "status": "Pending"
}
```

---

### 5. عرض جميع المشتريات

**Endpoint:** `GET /api/Purchases`

**الوصف:** عرض جميع المشتريات في النظام.

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "vehicleId": 1,
    "vehicleMake": "Toyota",
    "vehicleModel": "Camry",
    "vehicleYear": 2023,
    "purchasePrice": 28500.00,
    "purchaseDate": "2025-10-06T13:02:01.3521992Z",
    "status": "Pending"
  }
]
```

---

### 6. معالجة البيع

**Endpoint:** `PUT /api/Purchases/{id}/process`

**الوصف:** إتمام أو إلغاء عملية بيع.

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Path Parameters:**
- `id` (integer, required): معرف المشترى

**Request Body:**
```json
{
  "purchaseId": 1,
  "status": "Completed" // أو "Cancelled"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "vehicleId": 1,
  "vehicleMake": "Toyota",
  "vehicleModel": "Camry",
  "vehicleYear": 2023,
  "purchasePrice": 28500.00,
  "purchaseDate": "2025-10-06T13:02:01.3521992Z",
  "status": "Completed"
}
```

**ملاحظة:** عند إتمام البيع (Completed)، تُحدث حالة المركبة تلقائياً إلى غير متاح (isAvailable = false).

---

## المستخدمين (Users)

### 1. عرض جميع العملاء

**Endpoint:** `GET /api/Users/customers`

**الوصف:** عرض قائمة بجميع العملاء المسجلين.

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Response (200 OK):**
```json
[
  {
    "id": 2,
    "username": "customer1",
    "email": "customer@example.com",
    "createdAt": "2025-10-06T13:01:04.3464336Z",
    "purchaseCount": 1
  }
]
```

---

### 2. عرض جميع المستخدمين

**Endpoint:** `GET /api/Users`

**الوصف:** عرض قائمة بجميع المستخدمين (Admin و Customer).

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "username": "admin",
    "email": "admin@cardealership.com",
    "role": "Admin",
    "createdAt": "2025-10-06T13:01:04.3464336Z",
    "purchaseCount": 0
  },
  {
    "id": 2,
    "username": "customer1",
    "email": "customer@example.com",
    "role": "Customer",
    "createdAt": "2025-10-06T13:01:04.3464336Z",
    "purchaseCount": 1
  }
]
```

---

### 3. عرض تفاصيل مستخدم محدد

**Endpoint:** `GET /api/Users/{id}`

**الوصف:** الحصول على تفاصيل مستخدم محدد مع سجل مشترياته.

**الصلاحيات:** Admin فقط

**Headers:**
```
Authorization: Bearer {admin_token}
```

**Path Parameters:**
- `id` (integer, required): معرف المستخدم

**Response (200 OK):**
```json
{
  "id": 2,
  "username": "customer1",
  "email": "customer@example.com",
  "role": "Customer",
  "createdAt": "2025-10-06T13:01:04.3464336Z",
  "purchases": [
    {
      "id": 1,
      "vehicleId": 1,
      "purchasePrice": 28500.00,
      "purchaseDate": "2025-10-06T13:02:01.3521992Z",
      "status": "Completed"
    }
  ]
}
```

---

## رموز الحالة (Status Codes)

| الرمز | الوصف |
|------|-------|
| 200 OK | الطلب نجح |
| 201 Created | تم إنشاء المورد بنجاح |
| 400 Bad Request | بيانات الطلب غير صحيحة |
| 401 Unauthorized | المصادقة مطلوبة أو فشلت |
| 403 Forbidden | ليس لديك صلاحية للوصول |
| 404 Not Found | المورد غير موجود |
| 500 Internal Server Error | خطأ في الخادم |

---

## ملاحظات هامة

### استخدام JWT Token

بعد تسجيل الدخول أو التسجيل بنجاح، ستحصل على JWT token. استخدمه في جميع الطلبات المحمية:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### صلاحية OTP

- **المدة**: 5 دقائق
- **الاستخدام**: مرة واحدة فقط
- **التوليد**: عشوائي (6 أرقام)

### الحصول على OTP

في بيئة التطوير، يتم طباعة OTP في Console logs. في بيئة الإنتاج، سيتم إرساله عبر SMS أو Email.

لعرض OTP من logs:
```bash
tail -f /tmp/api_output.log | grep "OTP Code"
```

---

**نهاية التوثيق**
