Funcionalidade: Serviço de Pagamento
  Como usuário
  Quero realizar um pagamento
  Para que meu pedido seja processado

  Cenário: Pagamento realizado com sucesso
    Dado que existe um pedido com id "12345"
    Quando eu solicito o pagamento para o pedido "12345"
    Então a resposta do pagamento deve conter um PaymentId, QrData e QrCodeBase64