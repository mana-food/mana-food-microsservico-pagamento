# language: pt-BR

Funcionalidade: Processamento de Pagamentos via QR Code
  Como um sistema de pagamentos
  Eu quero gerar QR Codes e processar confirmações do MercadoPago
  Para que os pedidos sejam pagos de forma segura

Cenário: Gerar QR Code para um pedido válido
  Dado que existe um pedido com ID "3fa85f64-5717-4562-b3fc-2c963f66afa6" no Order Service
  E o pedido contém 2 itens no valor total de R$ 85,50
  Quando eu solicito a geração do QR Code para pagamento
  Então o sistema deve retornar um QR Code válido
  E o paymentId deve ser retornado
  E o QrData deve ser retornado
  E o QrCodeBase64 deve conter dados válidos

Cenário: Processar webhook de pagamento aprovado
  Dado que o MercadoPago enviou uma confirmação de pagamento com ID "12345678"
  E o pagamento está com status "approved" na API do MercadoPago
  E o external_reference é "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  Quando o webhook processa a confirmação
  Então o sistema deve registrar a aprovação do pagamento
  E deve extrair o OrderId do external_reference

Cenário: Rejeitar webhook de pagamento não aprovado
  Dado que o MercadoPago enviou uma confirmação de pagamento com ID "12345679"
  E o pagamento está com status "rejected" na API do MercadoPago
  Quando o webhook processa a confirmação
  Então o sistema não deve processar o pagamento
  E deve registrar o status como não aprovado

Cenário: Falha ao gerar QR Code quando pedido não existe
  Dado que não existe um pedido com ID "99999999-9999-9999-9999-999999999999" no Order Service
  Quando eu tento gerar o QR Code para pagamento
  Então o sistema deve retornar um erro informando que o pedido não foi encontrado
