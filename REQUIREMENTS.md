# CedarDelivery — Requirements

CedarDelivery is a delivery marketplace for Lebanon. Customers order from local stores
(restaurants, groceries, pharmacies), and drivers deliver the orders. Admins approve stores
and drivers and watch the platform.

Status: **APPROVED** by Edwin on 2026-10-25. This replaces the requirements PDF.

---

## 1. Roles

| Role | Who | Main actions |
|------|-----|--------------|
| **Admin** | Platform staff | Approve stores and drivers, manage users, see all orders, analytics |
| **Merchant** | Store owner | Manage their store and products, accept/reject and prepare orders |
| **Driver** | Delivery person | Go online/offline, accept deliveries, update delivery status |
| **Customer** | Anyone who signs up | Browse stores, order, pay, track, review |

- Everyone signs up as a Customer (email/password or Google).
- A Customer can **apply** to become a Merchant or a Driver. An Admin approves the application
  (same idea as CedarPay's AgentApplication).

## 2. Features (MVP = must have, Extra = if time allows)

### Accounts (MVP)
- Register, login, Google login, email confirmation, forgot password (Identity + MailKit).
- Profile: name, phone, profile picture.
- Saved delivery addresses (label like "Home", city, street, building, floor, notes, map pin).
- Dark mode, responsive layout (same as CedarPay).

### Stores and products (MVP)
- Store: name, description, category (Restaurant, Grocery, Pharmacy, Other), logo, address,
  map location, phone, opening hours, flat delivery fee, minimum order, open/closed switch.
- Store needs Admin approval before it shows up to customers.
- Products: name, description, price, image, product category inside the store (e.g. "Burgers"),
  available/unavailable switch.
- Customers browse stores by category, search by name, and open a store's menu.

### Cart and ordering (MVP)
- Cart holds products from **one store at a time**.
- Checkout: pick an address, add a note, choose payment method.
- Price = subtotal + store delivery fee (USD).
- Order status flow:
  `Pending → Accepted → Preparing → ReadyForPickup → PickedUp → Delivered`
  plus `Rejected` (by store) and `Cancelled` (by customer, only while Pending).
- Every status change is saved with a timestamp (order timeline).
- Customer sees order history and order details.

### Delivery (MVP)
- Driver profile: vehicle type (Motorcycle, Car, Bicycle), plate number, online/offline.
- When an order is `ReadyForPickup`, online drivers see it and one driver accepts it.
- Driver marks `PickedUp` and `Delivered`.
- Driver sees their delivery history and earnings (delivery fees of completed orders).

### Real-time (MVP, Phase 5)
- SignalR: the customer's order page updates live when the status changes.
- SignalR: merchants get a live alert for new orders; drivers for new available deliveries.
- Extra: driver shares live location, and the customer sees it on a map.

### Payments (MVP: cash, Extra: card)
- MVP: Cash on delivery.
- Extra: card payment with Stripe test mode (same as CedarPay), placeholder keys only.

### Reviews (MVP)
- After delivery, the customer rates the store (1–5 + comment) and the driver (1–5).
- Stores and drivers show their average rating.

### Notifications (MVP)
- In-app notifications (new order, status changes, application approved/rejected).
- Email for order confirmation and application decisions.

### Admin (MVP)
- Approve/reject Merchant and Driver applications.
- Manage users (activate/deactivate).
- See all orders with filters.
- Simple analytics: orders per day, revenue, top stores, active drivers.
- Activity log of user actions (same as CedarPay).

### Extra (only if time allows)
- Promo codes (percent or fixed discount, expiry date, usage limit).
- Map page showing nearby stores.
- AI helper that writes a product description from its name (Gemini, placeholder key).

## 3. REST API (under `/api`, JSON, paginated lists)
- `/api/stores`: list (filter by category, search), details, menu.
- `/api/orders`: create, list mine, details, update status.
- `/api/deliveries`: available, accept, update status.
- `/api/reviews`: create, list by store/driver.
- `/api/notifications`: list mine, unread count, mark read.
- Same JSON error format everywhere: `{ "success": false, "message": "...", "errors": [...] }`.

## 4. Data model (what Phase 1 will create)

All app tables use `int` IDs; users use Identity's `string` ID.

| Entity | Key fields | Relationships |
|--------|-----------|---------------|
| **User** (extends IdentityUser) | FirstName, LastName, ProfilePictureUrl, IsActive, CreatedAt | has many Addresses, Orders, Reviews, Notifications |
| **Address** | Label, City, Street, Building, Floor, Notes, Latitude, Longitude, IsDefault | belongs to User |
| **Store** | Name, Description, Category, LogoUrl, Address, Latitude, Longitude, Phone, OpensAt, ClosesAt, DeliveryFee, MinimumOrder, IsOpen, IsApproved, CreatedAt | owned by a User (Merchant); has many Products, Orders, Reviews |
| **ProductCategory** | Name, DisplayOrder | belongs to Store; has many Products |
| **Product** | Name, Description, Price, ImageUrl, IsAvailable | belongs to Store and ProductCategory |
| **CartItem** | Quantity | belongs to User and Product |
| **Order** | OrderNumber, Status, Subtotal, DeliveryFee, Total, PaymentMethod, PaymentStatus, Notes, delivery address snapshot (text + lat/lng), CreatedAt | belongs to Customer (User) and Store; has many OrderItems and StatusHistory; has one Delivery |
| **OrderItem** | ProductName, UnitPrice, Quantity (copied at order time) | belongs to Order and Product |
| **OrderStatusHistory** | Status, ChangedAt, ChangedByUserId, Note | belongs to Order |
| **DriverProfile** | VehicleType, PlateNumber, IsOnline, CurrentLatitude, CurrentLongitude, IsApproved | one-to-one with User |
| **Delivery** | Status, AssignedAt, PickedUpAt, DeliveredAt, DriverEarning | one-to-one with Order; belongs to DriverProfile |
| **RoleApplication** | RequestedRole (Merchant/Driver), Details, Status (Pending/Approved/Rejected), ReviewedByUserId, ReviewedAt | belongs to User |
| **Review** | TargetType (Store/Driver), Rating, Comment, CreatedAt | belongs to Customer and Order; points to a Store or a DriverProfile |
| **Notification** | Title, Message, Type, IsRead, RedirectUrl, CreatedAt | belongs to User |
| **ActivityLog** | Action, Details, IpAddress, UserAgent, CreatedAt | belongs to User |
| *PromoCode (Extra)* | Code, DiscountType, Value, ExpiresAt, MaxUses, UsedCount | used by Orders |

- Money fields use `decimal(18,2)`. Coordinates use `decimal(9,6)`.
- Enums: StoreCategory, OrderStatus, PaymentMethod, PaymentStatus, VehicleType,
  DeliveryStatus, ApplicationStatus, ReviewTarget, NotificationType, ActivityType.
- OrderItem copies the product name and price, so old orders don't change when a product is edited.

## 5. Out of scope
- Real payment processing (Stripe test mode only).
- Mobile app (responsive web only).
- Automatic driver assignment and route optimization (drivers accept orders manually).
- Multiple currencies (USD only).

## 6. Decisions
1. Roles: **Admin / Merchant / Driver / Customer**.
2. Drivers earn the full delivery fee. It is stored per delivery (`Delivery.DriverEarning`) so the rule can change later.
3. MVP is cash on delivery only. Stripe card payment is an Extra.
4. Live driver location is an Extra. The coordinate columns already exist in the data model.
