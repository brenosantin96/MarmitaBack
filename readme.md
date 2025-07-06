# MarmitaDelivery API

This is a RESTful API for managing frozen lunchbox deliveries. It handles user accounts, product listings, shopping cart functionality, order scheduling, and delivery options.

---

## Entities & Relationships (Simplified UML)

### `Lunchbox`
Represents a single meal product available for purchase.


---

### `Category`
Represents a group of lunchboxes (e.g. "Low Carb", "Vegetarian").


---

### `User`
Represents a registered customer (or admin).


---

### `Cart`
Represents a shopping cart tied to a user session. Becomes an order upon checkout.


---

### `CartItem`
Represents a lunchbox added to the cart.


---

### `Address`
Represents a user's saved delivery address.


---

### `Order`
Represents a confirmed purchase and scheduled delivery.


---

## Relationships Summary

- `User` can have many `Address`
- `User` can have many `Cart`
- `Cart` can have many `CartItem`
- `CartItem` can have many `Lunchbox`
- `Lunchbox` can have one `Category`
- `Cart` can have one `Order`
- `Order` can have one `Address`

---

## Design Notes

- A cart can only be created when user is authenticated. 
- An `Order` is only created after scheduling and payment.
- The model supports both delivery and in-person pickup.

---

## Technologies Used

- ASP.NET Core (C#)
- Entity Framework Core
- SQL Server / MySQL
- JWT Authentication
- RESTful API principles

---

## TODO

- [ ]  Order Scheduling and Checkout
- [ ]  Payment phase
- [ ]  Admin Panel for Product Management

---

## Contact

Developed by Breno Santin  
https://github.com/brenosantin96

