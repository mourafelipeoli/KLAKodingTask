# DeliveriesController

`POST /api/deliveries`: sends a conversion result to the signed-in account, by email or
WhatsApp. Requires a session and the antiforgery header; everything else is
`DeliveryService`.
