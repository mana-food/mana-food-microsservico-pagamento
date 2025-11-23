# mana-food-microsservico-pagamento

Microsserviço de pagamento do projeto ManaFood (novo). Este serviço contém implementação independente baseada no módulo de pagamento existente em 'mana-food-clean-architecture'.

EndPoints:
- POST /api/payment/create { orderId }

Observações:
- Implementação inicial usa um repositório em memória para orders. Ajustar para conectar ao DB ou consumir evento/HTTP para obter order real.
- Configurar seção PaymentProvider no appsettings com credenciais do MercadoPago.
