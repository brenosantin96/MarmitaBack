# 🍱 MarmitaDelivery API

**MarmitaDelivery** is a RESTful API developed with **ASP.NET Core** for managing the sales and delivery system of frozen meals.  
It covers everything from user and address registration to order creation, cart management, meal kits, payment methods, and delivery scheduling.

---

## 🧩 Entity Structure (Models)

### 🧍‍♂️ `User`
Represents a registered user (customer or administrator).

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Unique identifier |
| `Name` | string | User's name |
| `Email` | string | User's email |
| `Password` | string? | Password (securely stored) |
| `isAdmin` | bool | Indicates if user is administrator |
| `Addresses` | ICollection\<Address\> | Associated addresses |
| `DeliveryInfos` | ICollection\<DeliveryInfo\> | Associated delivery information |

---

### 🏠 `Address`
Represents an address registered by the user.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Unique identifier |
| `UserId` | int | Relationship with `User` |
| `ZipCode` | string | ZIP code |
| `Street` | string | Street name |
| `City` | string | City |
| `State` | string | State |
| `Neighborhood` | string | Neighborhood |
| `Number` | string | Street number |
| `Complement` | string | Address complement |
| `CreatedAt` | DateTime | Creation date |
| `UpdatedAt` | DateTime | Last update date |

---

### 🍽️ `Lunchbox`
Represents an individual meal available for purchase.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `Name` | string | Meal name |
| `Description` | string | Description |
| `Price` | decimal | Unit price |
| `ImageUrl` | string | Image URL |
| `PortionGram` | int | Weight in grams |
| `CategoryId` | int | Associated category |
| `KitLunchboxes` | List\<KitLunchbox\> | Relationship with kits |

---

### 🗂️ `Category`
Represents a meal category (e.g., "Low Carb", "Vegetarian").

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `Name` | string | Category name |
| `Lunchboxes` | List\<Lunchbox\> | Associated meals |

---

### 🎁 `Kit`
Represents a bundle of meals sold together (combo).

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `Name` | string | Kit name |
| `Description` | string | Description |
| `Price` | decimal | Total price |
| `ImageUrl` | string | Illustrative image |
| `CategoryId` | int | Associated category |
| `KitLunchboxes` | List\<KitLunchbox\> | Meals included in the kit |

---

### 🍱 `KitLunchbox`
Junction table between `Kit` and `Lunchbox`.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `KitId` | int | Relationship with `Kit` |
| `LunchboxId` | int | Relationship with `Lunchbox` |
| `Quantity` | int | Quantity of this meal in the kit |

---

### 🛒 `Cart`
Represents a user's active shopping cart.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `UserId` | int | Cart owner user |
| `CreatedAt` | DateTime | Creation date |
| `IsCheckedOut` | bool | Indicates if cart was finalized |
| `CartItems` | List\<CartItem\> | Cart items |

---

### 🧾 `CartItem`
Individual item within a cart (can be a single meal or a kit).

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `CartId` | int | Associated cart |
| `LunchboxId` | int? | Individual meal (optional) |
| `KitId` | int? | Kit (optional) |
| `Quantity` | int | Quantity |
> 💡 **Note:** Only one field (`LunchboxId` or `KitId`) should be filled per item.

---

### 🚚 `DeliveryInfo`
Represents delivery or pickup information for the order.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `UserId` | int | Associated user |
| `AddressId` | int? | Chosen address |
| `DeliveryType` | string | "Delivery" or "Pickup" |
| `DeliveryDate` | DateTime | Delivery date |
| `DeliveryPeriod` | string | "morning", "afternoon" or "evening" |
| `CanLeaveAtDoor` | bool | Can leave at door |
| `Order` | Order? | Related order (1:1) |

---

### 💳 `PaymentMethod`
Available payment method.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `Name` | string | Method name ("PIX", "Credit Card", etc.) |
| `Description` | string? | Additional instructions |
| `Orders` | ICollection\<Order\>? | Orders paid with this method |

---

### 📦 `Order`
Represents a finalized order (checkout completed).

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Identifier |
| `CartId` | int | Base cart for the order |
| `DeliveryInfoId` | int | Delivery information |
| `PaymentMethodId` | int | Payment method |
| `FullName` | string | Customer's full name |
| `Phone` | string | Contact phone |
| `Subtotal` | decimal | Product value |
| `DeliveryFee` | decimal | Delivery fee |
| `Total` | decimal | Order total value |
| `CreatedAt` | DateTime | Creation date |

---

## 🔗 Main Relationships

- **User** → has many **Addresses**
- **User** → has many **DeliveryInfos**
- **Cart** → belongs to **User**
- **Cart** → has many **CartItems**
- **CartItem** → can reference **Lunchbox** *or* **Kit**
- **Kit** → belongs to a **Category**
- **Kit** → contains multiple **Lunchboxes**
- **Order** → belongs to one **Cart**, one **DeliveryInfo** and one **PaymentMethod**

---

## ⚙️ Technologies Used

- **ASP.NET Core 8.0**
- **Entity Framework Core**
- **JWT Authentication**
- **SQL Server / MySQL**
- **C# 12**
- **RESTful API Architecture**

---

## 🚧 Features in Development

- [ ] Checkout and order creation
- [ ] Automatic delivery fee calculation
- [ ] PIX payment system integration
- [ ] Admin panel for product and category management

---

## 👨‍💻 Author

Developed by **Breno Santin**  
[GitHub: brenosantin96](https://github.com/brenosantin96)

---