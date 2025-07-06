/*

1.Front - end envia explicitamente o deliveryInfoId
Mais simples e direto.
Você garante no front-end que o DeliveryInfo escolhido pertence ao usuário autenticado e envia esse ID junto com o POST /orders.

{
  "cartId": 3,
  "deliveryInfoId": 5,
  "fullName": "João da Silva",
  "phone": "119999999",
  "paymentMethod": "PIX",
  "subtotal": 70,
  "deliveryFee": 5,
  "total": 75
}

No back-end, você faz:
1 - Valida se o DeliveryInfoId existe.
2 - Valida se o DeliveryInfo pertence a um endereço do usuário autenticado.
3 - (Opcional) Valida que ele ainda não está associado a um Order.
*/