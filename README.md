# 🏊 DreamsPools API

## هيكل المشروع

```
DreamsPools.API/
├── Controllers/
│   ├── AuthController.cs           ← تسجيل/دخول (عملاء، مندوبين، ادمن)
│   ├── AppointmentsController.cs   ← إدارة المواعيد كاملة
│   ├── OrdersController.cs         ← إدارة الطلبات + الفواتير
│   ├── ProductsController.cs       ← المنتجات + التصنيفات
│   ├── DashboardController.cs      ← التقارير المالية الكاملة
│   └── AdminControllers.cs         ← المندوبين، المصروفات، الإشعارات، الإعدادات
├── Models/
│   ├── BaseEntity.cs               ← الكلاس الأساسي (Id, CreatedAt, IsDeleted)
│   ├── User.cs                     ← العملاء
│   ├── Agent.cs                    ← المندوبين
│   ├── Admin.cs                    ← المسؤولين
│   ├── Category.cs                 ← التصنيفات
│   ├── Product.cs                  ← المنتجات (مع VAT تلقائي)
│   ├── Address.cs                  ← عناوين العملاء
│   ├── Order.cs                    ← الطلبات + العناصر
│   ├── Appointment.cs              ← المواعيد
│   ├── Financial.cs                ← المعاملات، الفواتير، المصروفات
│   └── Others.cs                   ← كوبونات، تقييمات، إشعارات، إعدادات
├── Data/
│   └── AppDbContext.cs             ← قاعدة البيانات + Seed Data
├── DTOs/                           ← كل البيانات المرسلة والمستقبلة
├── Helpers/
│   └── JwtHelper.cs               ← JWT + ApiResponse + NumberGenerator
└── appsettings.json                ← الإعدادات (DB, JWT, Firebase)
```

## 🚀 خطوات التشغيل

### 1. تحديث Connection String
في `appsettings.json`:
```json
"DefaultConnection": "Server=.;Database=DreamsPoolsDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 2. تثبيت الحزم
```bash
dotnet restore
```

### 3. إنشاء قاعدة البيانات
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. تشغيل المشروع
```bash
dotnet run
```

### 5. Swagger
افتح: `https://localhost:5001`

---

## 🔑 بيانات الادمن الافتراضية
- **Email:** admin@dreamspools.com
- **Password:** Admin@123

---

## 📊 الـ Endpoints الرئيسية

### Auth
| Method | Endpoint | الوصف |
|--------|----------|-------|
| POST | /api/auth/register | تسجيل عميل جديد |
| POST | /api/auth/login | دخول العميل |
| POST | /api/auth/agent/login | دخول المندوب |
| POST | /api/auth/admin/login | دخول الادمن |

### Appointments
| Method | Endpoint | الوصف |
|--------|----------|-------|
| POST | /api/appointments | حجز موعد |
| GET | /api/appointments/my | مواعيد العميل |
| PUT | /api/appointments/{id}/status | تحديث الحالة (ادمن) |
| GET | /api/appointments/agent/my | مواعيد المندوب |

### Orders
| Method | Endpoint | الوصف |
|--------|----------|-------|
| POST | /api/orders | إنشاء طلب |
| GET | /api/orders/my | طلبات العميل |
| PUT | /api/orders/{id}/status | تحديث الحالة (ادمن) |

### Dashboard (الادمن فقط)
| Method | Endpoint | الوصف |
|--------|----------|-------|
| GET | /api/dashboard/summary | ملخص مالي |
| GET | /api/dashboard/revenue | تقرير الإيرادات |
| GET | /api/dashboard/vat-report | تقرير الضريبة |
| GET | /api/dashboard/agents-performance | أداء المندوبين |
| GET | /api/dashboard/top-products | أكثر المنتجات مبيعاً |

---

## 💰 النظام المالي

### الضريبة (VAT)
- تُحسب تلقائياً **15%** على كل طلب
- فاتورة ضريبية بترقيم تلقائي `DP-INV-2024-00001`
- تقرير ضريبي شهري جاهز

### الإيرادات
- مبيعات المنتجات + رسوم التوصيل
- يُخصم منها: الخصومات + المصروفات

### الفواتير
- `DP-2024-00001` للطلبات
- `DP-APT-2024-00001` للمواعيد
- `DP-INV-2024-00001` للفواتير الضريبية
- `DP-TRX-2024-00001` للمعاملات

---

## ⚙️ الإعدادات القابلة للتغيير (من لوحة التحكم)
- `VatPercentage` - نسبة الضريبة (افتراضي: 15%)
- `DeliveryFee` - رسوم التوصيل (افتراضي: 20 ريال)
- `AgentCommissionPercentage` - نسبة عمولة المندوب (افتراضي: 10%)
