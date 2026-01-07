# ManaFood - Microsserviço de Pagamento

Microsserviço **stateless** responsável pela gestão de pagamentos via Mercado Pago QR Code do sistema ManaFood.

## 🏗️ Arquitetura

Este serviço segue os princípios de **Clean Architecture**.

```
mana-food-microsservico-pagamento/
├── src/
│   ├── ManaFoodPayment.Api/           # Camada de apresentação (Controllers, Webhooks, configuração da API)
│   │   ├── Controllers/
│   │   ├── Webhooks/
│   │   ├── appsettings.json
│   │   └── Program.cs
│   ├── ManaFoodPayment.Application/   # Camada de aplicação (DTOs, interfaces)
│   │   ├── Dtos/
│   │   ├── Interfaces/
│   │   └── UseCases/
│   ├── ManaFoodPayment.Domain/        # Camada de domínio (enums, regras de negócio)
│   │   └── Enums/
│   └── ManaFoodPayment.Infrastructure/ # Infraestrutura (comunicação externa, serviços)
│       ├── Configurations/
│       └── Services/
├── tests/
│   ├── ManaFoodPayment.UnitTests/     # Testes unitários (xUnit)
│   └── ManaFoodPayment.BDD/           # Testes BDD (SpecFlow)
├── k8s/                               # Manifests Kubernetes
├── docker-compose.yml
├── Dockerfile
└── README.md
```

### Descrição das Camadas

- **Api**: Controllers da API REST, webhooks do Mercado Pago, configurações do ASP.NET Core
- **Application**: DTOs para transferência de dados, interfaces de serviços
- **Domain**: Enums (PaymentMethod) e regras de negócio puras
- **Infrastructure**: Serviços de comunicação com MercadoPago API e Order Service (REST HTTP)

### Tecnologias

- **.NET 9.0** com Clean Architecture
- **Mercado Pago API** para geração de QR Codes e validação de status
- **QRCoder** para geração de imagens QR Code
- **xUnit** + **Moq** + **FluentAssertions** para testes unitários (23 testes)
- **SpecFlow** para testes BDD (4 cenários em Gherkin português)

## 📋 Pré-requisitos

- [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) e [Docker Compose](https://docs.docker.com/compose/)
- Conta no [Mercado Pago](https://www.mercadopago.com.br/developers)
- [Ngrok](https://ngrok.com/) para webhook (opcional, apenas para testes locais)
- **Order Service** rodando (para buscar dados dos pedidos)
---

## 🚀 Como Executar o Projeto

### 1. Clonando o repositório

```sh
git clone https://github.com/mana-food/mana-food-microsservico-pagamento.git
cd mana-food-microsservico-pagamento
```

### 2. Configurando o Webhook com Ngrok

A integração com Mercado Pago exige uma URL pública para receber notificações de pagamento. Utilizamos o **Ngrok** para criar um túnel seguro da internet para sua máquina local.

#### O que é o Ngrok?

Ngrok gera um link público (ex: `https://7cdbccf5ea42.ngrok-free.app`) que redireciona para seu servidor local (`http://localhost:5058`), mesmo que você esteja atrás de um firewall ou em uma rede privada.

#### Como usar o Ngrok:

1. **Instale o Ngrok**  
   Acesse [ngrok.com](https://ngrok.com/) e siga as instruções de instalação.

2. **Execute o Ngrok**  
   No terminal, rode:
   ```sh
   ngrok http https://localhost:5058
   ```

3. **Copie a URL gerada**  
   O Ngrok exibirá uma URL como `https://abc123.ngrok-free.app`. Copie-a para usar na configuração das variáveis de ambiente.

⚠️ **Importante**: O link Ngrok muda sempre que você reiniciar o serviço. Atualize a variável `MERCADOPAGO_NOTIFICATION_URL` com a nova URL sempre que necessário.

### 3. Configurando Variáveis de Ambiente

Configure as variáveis de ambiente do Mercado Pago antes de executar a aplicação. Estas devem ser definidas no sistema operacional:

**Linux/Mac:**
```sh
export MERCADOPAGO_ACCESS_TOKEN="seu_token_aqui"
export MERCADOPAGO_NOTIFICATION_URL="https://SEU_NGROK_ID.ngrok-free.app/api/webhooks/mercadopago/payment-confirmation"
export MERCADOPAGO_USER_ID="seu_user_id"
export MERCADOPAGO_STORE_ID="seu_store_id"
export MERCADOPAGO_EXTERNAL_STORE_ID="seu_external_store_id"
export MERCADOPAGO_EXTERNAL_POS_ID="seu_external_pos_id"
```

**Windows (PowerShell):**
```powershell
$env:MERCADOPAGO_ACCESS_TOKEN="seu_token_aqui"
$env:MERCADOPAGO_NOTIFICATION_URL="https://SEU_NGROK_ID.ngrok-free.app/api/webhooks/mercadopago/payment-confirmation"
$env:MERCADOPAGO_USER_ID="seu_user_id"
$env:MERCADOPAGO_STORE_ID="seu_store_id"
$env:MERCADOPAGO_EXTERNAL_STORE_ID="seu_external_store_id"
$env:MERCADOPAGO_EXTERNAL_POS_ID="seu_external_pos_id"
```

#### Como obter as credenciais do Mercado Pago:

1. Acesse [Mercado Pago Developers](https://www.mercadopago.com.br/developers/pt/docs/qr-code)
2. Crie uma aplicação e obtenha o `ACCESS_TOKEN`
3. Crie uma conta de teste com perfil **"cliente"** e forma de pagamento **"QRCode"** seguindo [este guia](https://www.mercadopago.com.br/developers/pt/docs/qr-code/additional-content/your-integrations/test/accounts)
4. Configure um ponto de venda (POS) para obter `USER_ID`, `STORE_ID`, `EXTERNAL_STORE_ID` e `EXTERNAL_POS_ID`

### 4. Executando com Docker Compose

Subindo a API:

```sh
docker-compose up --build
```

Acesse: `http://localhost:5058/swagger`

**Para parar os containers:**
```sh
docker-compose down
```

### 5. Executando com Kubernetes

1. **Pré-requisitos**  
   Certifique-se de ter o [Kubernetes](https://kubernetes.io/) e o [Minikube](https://minikube.sigs.k8s.io/docs/start/) instalados.

2. **Construa a imagem no Minikube**
   ```sh
   eval $(minikube docker-env)
   docker build -t manafood-payment-api:latest .
   ```

3. **Aplique os manifests Kubernetes**
   ```sh
   cd k8s
   kubectl apply -f configmap.yaml
   kubectl apply -f service.yaml
   kubectl apply -f deployment.yaml
   kubectl apply -f hpa.yaml
   ```

4. **Valide o status**
   ```sh
   kubectl get pods
   kubectl logs -f deployment/mana-food-payment
   minikube dashboard
   ```

---

## 🧪 Testes

### Executar todos os testes

```sh
dotnet test
```

### Executar com cobertura

```sh
dotnet test --collect:"XPlat Code Coverage"
```

### Estrutura de Testes

- **23 testes unitários**: Cobrem serviços, DTOs e lógica de negócio
- **4 testes BDD**: Cenários de negócio escritos em Gherkin (português)

## ⚙️ Configuração

### Arquivo appsettings.json

Edite `src/ManaFoodPayment.Api/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OrderService": {
    "BaseUrl": "http://order-api-service:8080"
  }
}
```

⚠️ **Atenção**: 
- Configure as credenciais do Mercado Pago via **variáveis de ambiente do sistema**, por segurança

---

## 📡 Endpoints Principais

### Pagamentos

- `POST /api/payment/create` - Criar pagamento com QR Code para um pedido
  ```json
  {
    "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
  ```
  **Resposta:**
  ```json
  {
    "paymentId": "12345678",
    "qrData": "00020101021243650016COM.MERCADOLIBRE...",
    "qrCodeBase64": "iVBORw0KGgoAAAANSUhEUgAA..."
  }
  ```

- `GET /api/payment/{orderId}/qr-image` - Obter imagem do QR Code em PNG (Base64)

### Webhooks

- `POST /api/webhooks/mercadopago/payment-confirmation` - Confirmação de pagamento do Mercado Pago
  ```json
  {
    "id": 12345678
  }
  ```
  **Comportamento**:
  - Consulta status no MercadoPago via API
  - Se status = "approved", extrai OrderId do external_reference
  - Loga a confirmação (stateless, sem persistência)

### Health Check

- `GET /healthz` - Health check do serviço.

---

## � Comunicação entre Microsserviços

### Order Service

O Payment Service se comunica com o Order Service via REST HTTP:

- **Endpoint**: `GET /api/orders/{orderId}`
- **Propósito**: Buscar detalhes do pedido (itens, preços, cliente) para gerar o QR Code
- **Configuração**: `OrderService:BaseUrl` no appsettings.json
- **Fluxo**:
  1. Cliente solicita pagamento com `orderId`
  2. Payment Service consulta Order Service (REST HTTP)
  3. Gera QR Code com dados do pedido
  4. Retorna QR Code para o cliente

### Mercado Pago

Comunicação direta com a API do Mercado Pago para:
- **Geração de QR Code**: `PUT /instore/orders/qr/seller/collectors/{userId}/pos/{posId}/qrs`
- **Validação de pagamento**: `GET /v1/payments/{paymentId}` (usado no webhook)
- **Webhook**: Recebe notificações quando pagamento é confirmado

### Fluxo Completo de Pagamento

```
1. Cliente → Payment Service: POST /api/payment/create {orderId}
2. Payment Service → Order Service: GET /api/orders/{orderId}
3. Order Service → Payment Service: OrderDto (items, prices, total)
4. Payment Service → MercadoPago: PUT /instore/orders/qr/... (criar QR Code)
5. MercadoPago → Payment Service: QR Code data + paymentId
6. Payment Service → Cliente: QR Code (Base64 PNG)
7. Cliente paga via App MercadoPago escaneando QR Code
8. MercadoPago → Payment Service: POST /api/webhooks/.../payment-confirmation
9. Payment Service → MercadoPago: GET /v1/payments/{paymentId} (validar status)
10. Payment Service: Loga "Pagamento aprovado para Order {orderId}"
```

---

### Dados Consultados

- **Order Service**: Busca dados via REST HTTP.
- **MercadoPago API**: Consulta status de pagamento em tempo real.

---

## 🔧 Comandos Úteis

### Build

```sh
dotnet build
```

### Testes

```sh
# Todos os testes
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Apenas Unit Tests
dotnet test tests/ManaFoodPayment.UnitTests

# Apenas BDD
dotnet test tests/ManaFoodPayment.BDD
```

### Docker

```sh
# Build da imagem
docker build -t manafood-payment-api:latest .

# Run local
docker run -p 5058:80 \
  -e MERCADOPAGO_ACCESS_TOKEN="seu_token" \
  -e MERCADOPAGO_NOTIFICATION_URL="https://seu-ngrok.com/api/webhooks/..." \
  -e MERCADOPAGO_USER_ID="123456" \
  -e MERCADOPAGO_STORE_ID="store123" \
  -e MERCADOPAGO_EXTERNAL_STORE_ID="ext123" \
  -e MERCADOPAGO_EXTERNAL_POS_ID="pos123" \
  manafood-payment-api:latest
```

---

## 📚 Documentação Complementar

### Notion - Documentação do Projeto
```
https://chartreuse-fountain-62d.notion.site/203ce57501598031b488df683ec4c8dd
```

### MIRO - Diagramas e Fluxos
```
https://miro.com/app/board/uXjVIHWEfCI=/
```

---

## 🏛️ Princípios de Clean Architecture

Este projeto segue os princípios de Clean Architecture com **arquitetura stateless**:

- ✅ **Independência de Frameworks**: A lógica de negócio não depende de frameworks externos
- ✅ **Testabilidade**: Todas as camadas são testáveis isoladamente
- ✅ **Independência de UI**: A camada de apresentação pode ser substituída sem afetar o core
- ✅ **Independência de Banco de Dados**: Sem persistência local (stateless)
- ✅ **Regras de Negócio Isoladas**: O domínio contém apenas regras de negócio puras (enums)
- ✅ **Comunicação via Interfaces**: Uso de injeção de dependência para todos os serviços

---

## 📄 Licença

Este projeto está sob a licença especificada no arquivo [LICENSE](LICENSE).

---