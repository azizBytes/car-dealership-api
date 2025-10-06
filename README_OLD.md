# Car Dealership API - نظام إدارة معرض السيارات

نظام شامل لإدارة معرض السيارات مبني على **.NET 9 Web API** مع دعم عمليات CRUD، التحكم بالوصول المبني على الأدوار، ونظام أمان OTP (كلمة مرور لمرة واحدة).

## المحتويات

- [نظرة عامة](#نظرة-عامة)
- [المميزات الرئيسية](#المميزات-الرئيسية)
- [البنية التقنية](#البنية-التقنية)
- [كيفية التشغيل](#كيفية-التشغيل)
- [نقاط النهاية المتاحة](#نقاط-النهاية-المتاحة)
- [أمثلة الاستخدام](#أمثلة-الاستخدام)
- [قرارات التصميم](#قرارات-التصميم)
- [البيانات النموذجية](#البيانات-النموذجية)

## نظرة عامة

يوفر هذا النظام واجهة برمجية متكاملة لإدارة معرض السيارات مع دعم دورين رئيسيين:

- **المدير (Admin)**: إدارة المركبات، معالجة المبيعات، عرض العملاء
- **العميل (Customer)**: تصفح المركبات، طلب الشراء، عرض سجل المشتريات

## المميزات الرئيسية

### 1. إدارة المستخدمين
- تسجيل حسابات جديدة مع التحقق بواسطة OTP
- تسجيل دخول آمن مع مصادقة ثنائية (OTP)
- دعم الأدوار (Admin/Customer)
- مصادقة JWT للجلسات

### 2. إدارة المركبات
- إضافة مركبات جديدة (Admin فقط)
- تحديث تفاصيل المركبات مع حماية OTP (Admin فقط)
- تصفح المركبات مع فلترة متقدمة
- عرض تفاصيل المركبة الكاملة
- حذف المركبات (Admin فقط)

### 3. إدارة المشتريات
- طلب شراء مركبة مع تأكيد OTP (Customer)
- معالجة المبيعات (إتمام/إلغاء) (Admin)
- عرض سجل المشتريات للعميل
- عرض جميع المشتريات (Admin)

### 4. نظام OTP الشامل
- توليد رموز OTP عشوائية (6 أرقام)
- صلاحية محددة (5 دقائق)
- محاكاة إرسال OTP عبر Console
- حماية العمليات الحساسة:
  - تسجيل الدخول
  - التسجيل
  - طلب الشراء
  - تحديث المركبة

## البنية التقنية

### التقنيات المستخدمة

- **.NET 9** - إطار العمل الرئيسي
- **ASP.NET Core Web API** - بناء واجهة برمجية RESTful
- **Entity Framework Core** - ORM لإدارة البيانات
- **In-Memory Database** - قاعدة بيانات للتطوير والاختبار
- **JWT Bearer Authentication** - نظام المصادقة
- **Swagger/OpenAPI** - توثيق API تفاعلي

### هيكل المشروع

```
CarDealershipAPI/
├── Controllers/          # نقاط النهاية API
│   ├── AuthController.cs
│   ├── VehiclesController.cs
│   ├── PurchasesController.cs
│   └── UsersController.cs
├── Models/              # نماذج البيانات
│   ├── User.cs
│   ├── Vehicle.cs
│   ├── Purchase.cs
│   └── OtpCode.cs
├── DTOs/                # كائنات نقل البيانات
│   ├── AuthDTOs.cs
│   ├── VehicleDTOs.cs
│   └── PurchaseDTOs.cs
├── Services/            # خدمات الأعمال
│   ├── OtpService.cs
│   ├── TokenService.cs
│   └── PasswordService.cs
├── Data/                # سياق قاعدة البيانات
│   └── ApplicationDbContext.cs
├── Program.cs           # نقطة الدخول والإعدادات
└── appsettings.json     # ملف الإعدادات
```

## كيفية التشغيل

### المتطلبات الأساسية

- .NET 9 SDK

### خطوات التشغيل

1. **استنساخ المشروع**
   ```bash
   cd CarDealershipAPI
   ```

2. **استعادة الحزم**
   ```bash
   dotnet restore
   ```

3. **بناء المشروع**
   ```bash
   dotnet build
   ```

4. **تشغيل التطبيق**
   ```bash
   dotnet run
   ```

5. **الوصول إلى Swagger UI**
   - افتح المتصفح على: `http://localhost:5050/swagger`
   - أو استخدم: `https://localhost:7050/swagger` (HTTPS)

## نقاط النهاية المتاحة

### المصادقة (Authentication)

#### تسجيل مستخدم جديد

**الخطوة 1: طلب التسجيل**
```http
POST /api/Auth/register/request
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass@123",
  "role": "Customer"
}
```

**الخطوة 2: التحقق من OTP وإكمال التسجيل**
```http
POST /api/Auth/register/verify?otpCode=123456
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass@123",
  "role": "Customer"
}
```

#### تسجيل الدخول

**الخطوة 1: طلب تسجيل الدخول**
```http
POST /api/Auth/login/request
Content-Type: application/json

{
  "email": "admin@cardealership.com",
  "password": "Admin@123"
}
```

**الخطوة 2: التحقق من OTP وإكمال تسجيل الدخول**
```http
POST /api/Auth/login/verify
Content-Type: application/json

{
  "email": "admin@cardealership.com",
  "otpCode": "123456"
}
```

### المركبات (Vehicles)

#### تصفح جميع المركبات (مصادقة مطلوبة)
```http
GET /api/Vehicles
Authorization: Bearer {token}

# مع فلترة اختيارية
GET /api/Vehicles?make=Toyota&minPrice=20000&maxPrice=50000&isAvailable=true
```

#### عرض تفاصيل مركبة محددة
```http
GET /api/Vehicles/{id}
Authorization: Bearer {token}
```

#### إضافة مركبة جديدة (Admin فقط)
```http
POST /api/Vehicles
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "make": "Toyota",
  "model": "Camry",
  "year": 2024,
  "price": 35000,
  "color": "Silver",
  "mileage": 0,
  "vin": "1HGBH41JXMN109999",
  "description": "Brand new Toyota Camry"
}
```

#### تحديث مركبة (Admin فقط - مع OTP)

**الخطوة 1: طلب التحديث**
```http
POST /api/Vehicles/update/request
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "vehicleId": 1,
  "price": 27000,
  "mileage": 16000
}
```

**الخطوة 2: التحقق والتحديث**
```http
PUT /api/Vehicles/update/verify?otpCode=123456
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "vehicleId": 1,
  "price": 27000,
  "mileage": 16000
}
```

#### حذف مركبة (Admin فقط)
```http
DELETE /api/Vehicles/{id}
Authorization: Bearer {admin_token}
```

### المشتريات (Purchases)

#### طلب شراء مركبة (Customer - مع OTP)

**الخطوة 1: طلب الشراء**
```http
POST /api/Purchases/request
Authorization: Bearer {customer_token}
Content-Type: application/json

{
  "vehicleId": 1
}
```

**الخطوة 2: التحقق وإكمال الطلب**
```http
POST /api/Purchases/verify?otpCode=123456
Authorization: Bearer {customer_token}
Content-Type: application/json

{
  "vehicleId": 1
}
```

#### عرض سجل المشتريات الخاص (Customer)
```http
GET /api/Purchases/my-purchases
Authorization: Bearer {customer_token}
```

#### عرض تفاصيل مشترى محدد
```http
GET /api/Purchases/{id}
Authorization: Bearer {token}
```

#### عرض جميع المشتريات (Admin فقط)
```http
GET /api/Purchases
Authorization: Bearer {admin_token}
```

#### معالجة البيع (Admin فقط)
```http
PUT /api/Purchases/{id}/process
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "purchaseId": 1,
  "status": "Completed"
}
```

### المستخدمين (Users - Admin فقط)

#### عرض جميع العملاء
```http
GET /api/Users/customers
Authorization: Bearer {admin_token}
```

#### عرض جميع المستخدمين
```http
GET /api/Users
Authorization: Bearer {admin_token}
```

#### عرض تفاصيل مستخدم محدد
```http
GET /api/Users/{id}
Authorization: Bearer {admin_token}
```

## أمثلة الاستخدام

### سيناريو 1: تسجيل عميل جديد وشراء مركبة

```bash
# 1. طلب التسجيل
curl -X POST http://localhost:5050/api/Auth/register/request \
  -H "Content-Type: application/json" \
  -d '{
    "username": "alice",
    "email": "alice@example.com",
    "password": "Alice@123",
    "role": "Customer"
  }'

# 2. التحقق من OTP (من Console logs)
curl -X POST "http://localhost:5050/api/Auth/register/verify?otpCode=123456" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "alice",
    "email": "alice@example.com",
    "password": "Alice@123",
    "role": "Customer"
  }'

# 3. تصفح المركبات المتاحة
curl -X GET "http://localhost:5050/api/Vehicles?isAvailable=true" \
  -H "Authorization: Bearer {token}"

# 4. طلب شراء مركبة
curl -X POST http://localhost:5050/api/Purchases/request \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"vehicleId": 1}'

# 5. التحقق من OTP وإكمال الشراء
curl -X POST "http://localhost:5050/api/Purchases/verify?otpCode=654321" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"vehicleId": 1}'
```

### سيناريو 2: مدير يضيف مركبة ويعالج البيع

```bash
# 1. تسجيل دخول المدير
curl -X POST http://localhost:5050/api/Auth/login/request \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@cardealership.com",
    "password": "Admin@123"
  }'

# 2. التحقق من OTP
curl -X POST http://localhost:5050/api/Auth/login/verify \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@cardealership.com",
    "otpCode": "789012"
  }'

# 3. إضافة مركبة جديدة
curl -X POST http://localhost:5050/api/Vehicles \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "make": "BMW",
    "model": "X7",
    "year": 2024,
    "price": 85000,
    "color": "Black",
    "mileage": 0,
    "vin": "BMW123456789",
    "description": "Luxury SUV"
  }'

# 4. عرض جميع المشتريات المعلقة
curl -X GET http://localhost:5050/api/Purchases \
  -H "Authorization: Bearer {admin_token}"

# 5. معالجة البيع
curl -X PUT http://localhost:5050/api/Purchases/1/process \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "purchaseId": 1,
    "status": "Completed"
  }'
```

## قرارات التصميم

### 1. نظام OTP

تم تطبيق نظام OTP شامل لحماية العمليات الحساسة. القرارات الرئيسية:

- **صلاحية 5 دقائق**: توازن بين الأمان وتجربة المستخدم
- **رموز 6 أرقام**: معيار صناعي للأمان
- **استخدام واحد**: كل OTP يُستخدم مرة واحدة فقط
- **محاكاة الإرسال**: Console output بدلاً من SMS/Email الفعلي للتطوير

### 2. المصادقة والتفويض

- **JWT Tokens**: مصادقة stateless مع صلاحية 24 ساعة
- **Role-based Access**: فصل واضح بين صلاحيات Admin و Customer
- **Password Hashing**: SHA256 لتخزين كلمات المرور بشكل آمن

### 3. قاعدة البيانات

- **In-Memory Database**: مناسبة للتطوير والاختبار
- **Entity Framework Core**: ORM قوي مع دعم LINQ
- **Seed Data**: بيانات نموذجية تُحمل تلقائياً عند البدء

### 4. معالجة الأخطاء

- **Input Validation**: التحقق من صحة المدخلات في جميع نقاط النهاية
- **Proper HTTP Status Codes**: استخدام رموز HTTP المناسبة
- **Descriptive Error Messages**: رسائل خطأ واضحة للمطورين

### 5. توثيق API

- **Swagger/OpenAPI**: توثيق تفاعلي كامل
- **JWT Integration**: دعم المصادقة مباشرة من Swagger UI
- **XML Comments**: توثيق كامل لجميع نقاط النهاية

## البيانات النموذجية

### حسابات المستخدمين

| الدور | البريد الإلكتروني | كلمة المرور |
|------|-------------------|-------------|
| Admin | admin@cardealership.com | Admin@123 |
| Customer | customer@example.com | Customer@123 |

### المركبات النموذجية (10 مركبات)

1. **Toyota Camry 2023** - $28,500
2. **Honda Accord 2022** - $26,800
3. **Ford F-150 2024** - $45,000
4. **Tesla Model 3 2023** - $42,000
5. **BMW X5 2022** - $58,000
6. **Mercedes-Benz C-Class 2023** - $48,500
7. **Chevrolet Silverado 2023** - $42,500
8. **Nissan Altima 2022** - $24,500
9. **Hyundai Tucson 2023** - $32,000
10. **Mazda CX-5 2024** - $35,500

## الافتراضات والقيود

### الافتراضات

1. **OTP Delivery**: يتم محاكاة إرسال OTP عبر Console (في بيئة الإنتاج، يُستخدم SMS/Email)
2. **Payment Processing**: لا يوجد تكامل مع بوابات الدفع (خارج نطاق المشروع)
3. **Image Upload**: لا يوجد دعم لرفع صور المركبات حالياً
4. **Email Verification**: لا يتم التحقق من صحة البريد الإلكتروني الفعلي

### القيود

1. **In-Memory Database**: البيانات تُفقد عند إعادة تشغيل التطبيق
2. **Single Instance**: لا يوجد دعم للتوسع الأفقي (Horizontal Scaling)
3. **Rate Limiting**: لا يوجد حد لمعدل الطلبات حالياً
4. **File Storage**: لا يوجد نظام لتخزين الملفات

## التحسينات المستقبلية

1. **قاعدة بيانات دائمة**: الانتقال إلى SQL Server أو PostgreSQL
2. **تكامل SMS/Email**: إرسال OTP الفعلي
3. **Rate Limiting**: حماية من الهجمات
4. **Caching**: تحسين الأداء باستخدام Redis
5. **Logging**: نظام Logging متقدم (Serilog)
6. **Unit Tests**: اختبارات شاملة
7. **Docker**: Containerization للنشر السهل
8. **CI/CD**: أتمتة النشر

## الترخيص

هذا المشروع تعليمي ومفتوح المصدر.

## الدعم

للمساعدة أو الاستفسارات، يرجى فتح Issue في المستودع.

---

**تم التطوير باستخدام .NET 9 و ASP.NET Core Web API**
