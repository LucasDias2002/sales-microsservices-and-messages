# 🛒 Sales

Sistema de vendas desenvolvido com **C# e .NET**, utilizando uma arquitetura baseada em **microsserviços**, **RabbitMQ** e comunicação assíncrona entre os serviços.

O projeto foi desenvolvido com o objetivo de aplicar na prática conceitos de **arquitetura de microsserviços, mensageria e processamento assíncrono**, explorando a comunicação entre diferentes serviços e o gerenciamento de uma transação distribuída utilizando o padrão **Saga Orchestrated**.

Tecnologias: 
<p align="left">
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/csharp/csharp-original.svg" width="40" height="40" alt="C#" />
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/dotnetcore/dotnetcore-original.svg" width="40" height="40" alt=".NET" />
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/docker/docker-original.svg" width="40" height="40" alt="Docker" />
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/rabbitmq/rabbitmq-original.svg" width="40" height="40" alt="RabbitMQ" />
</p>

---

## 🏗️ Arquitetura

O sistema é composto por três microsserviços:

![Arquitetura](Docs/images/arquitetura-projeto.png)

* **Order Service** — Responsável pelo gerenciamento dos pedidos.
* **Inventory Service** — Responsável pelo gerenciamento e reserva de estoque.
* **Payment Service** — Responsável pelo processamento dos pagamentos.

Cada microsserviço possui responsabilidades próprias e se comunica com os demais por meio de chamadas síncronas e/ou mensagens assíncronas, de acordo com cada etapa do fluxo.

### Serviços

| Microsserviço         | Responsabilidade                    |
| --------------------- | ----------------------------------- |
| **Order Service**     | Criação e gerenciamento dos pedidos |
| **Inventory Service** | Controle e reserva de estoque       |
| **Payment Service**   | Processamento dos pagamentos        |

---

## 📨 Mensageria

A comunicação assíncrona entre os microsserviços é realizada utilizando **RabbitMQ**.

O projeto utiliza o exchange:

```text
EcommerceEvents
```

O sistema possui **mais de 7 filas**, utilizadas para distribuir os eventos entre os serviços de acordo com as diferentes etapas do processamento do pedido.

### Filas do projeto:

|                 RABBITMQ QUEUES              |
| -------------------------------------------- |
| inventory_order_created_queue                |
| inventory_payment_processed_queue            |
| order_payment_processed_queue                |
| order_stock_not_reservation_queue            |
| order_stock_reserved_queue                   |
| payment_stock_released_queue                 |
| payment_stock_reserved_queue                 |

A utilização de mensageria permite que os serviços se comuniquem de forma desacoplada, evitando que todo o fluxo dependa exclusivamente de chamadas HTTP síncronas.

---

## 🔄 Saga Coreografada

Para coordenar o processamento de uma transação distribuída, o projeto utiliza o padrão **Saga Choreography (Saga Coreografada)**.

Nesse modelo, não existe um serviço central responsável por controlar todo o fluxo. Cada microsserviço é responsável por executar sua própria operação, publicar um evento quando a operação é concluída e reagir aos eventos publicados pelos demais serviços.

A comunicação entre os serviços acontece de forma assíncrona através do **RabbitMQ**.

De forma simplificada:

```text
Order Service
     │
     │ OrderCreated
     ▼
Inventory Service
     │
     │ StockReserved
     ▼
Payment Service
     │
     │ PaymentProcessed
     ▼
Order Service
```

Cada etapa da Saga é desencadeada pelo evento gerado pela etapa anterior.

### Características

* Comunicação assíncrona através do RabbitMQ
* Serviços desacoplados
* Cada serviço possui sua própria responsabilidade
* Cada serviço reage aos eventos de interesse
* Não existe um orquestrador central
* O fluxo é coordenado através dos eventos publicados pelos próprios serviços
* Permite implementar operações de compensação em caso de falhas

Esse modelo permite que os microsserviços participem de uma transação distribuída sem depender de uma única aplicação central para controlar todo o processo.

---

O fluxo completo da Saga e suas respectivas mensagens será detalhado abaixo.

---

## 🗄️ Persistência

Os microsserviços possuem persistência independente, seguindo o princípio de **Database per Service**.

* **Order Service** → Banco de dados próprio
* **Inventory Service** → Banco de dados próprio
* **Payment Service** → Não possui banco de dados próprio

A separação dos bancos permite que cada serviço mantenha autonomia sobre seus próprios dados, reduzindo o acoplamento entre os microsserviços.

---

## 🛠️ Tecnologias

### Back-end

* **C#**
* **.NET**
* **ASP.NET Core**
* **Entity Framework Core**

### Mensageria

* **RabbitMQ**

### Banco de dados

* **SQL Server**

### Infraestrutura

* **Docker**

### Outros conceitos e ferramentas

* APIs REST
* Microsserviços
* Comunicação assíncrona
* Mensageria
* Saga Coreografada
* Event-driven architecture
* Dependency Injection

---

## 🐳 Executando o projeto

### Subindo a infraestrutura

Na raiz do projeto, execute:

```text
docker compose up --build
```

Isso irá iniciar os serviços de infraestrutura necessários:

* RabbitMQ
* SQL Server do OrderService
* SQL Server do InventoryService
* SQL Server do PaymentService
* OrderService
* InventoryService
* PaymentService

Para verificar se os containers estão executando:

```text
docker compose ps
```

Cada microsserviço será dentro de um container docker.

### Configuração das conexões

Como os microsserviços são executados diretamente na máquina, as conexões devem utilizar localhost.

|Serviço         |	Banco        |	Porta |
| ---------------|---------------|------ |
|**OrderService**    |	order-db     |	1434  |
|**InventoryService**|	inventory-db | 1435  |
|**PaymentService**  |	payment-db   |	1436  |
|**RabbitMQ**        |	rabbitmq	   | 5672  |

Exemplo de conexão do OrderService:

```text
Server=order-db,1433;Database=Sales.OrderService;User Id=sa;Password=YourStrong@Password123;Encrypt=False;TrustServerCertificate=True;
```

Para o RabbitMQ:
```text
rabbitmq:5672
```

### RabbitMQ Management

A interface de gerenciamento do RabbitMQ estará disponível em:

http://localhost:15672

Credenciais padrão:

* Usuário: guest
* Senha: guest

### Parando a infraestrutura

Para parar os containers:

```text
docker compose down
```

Os dados dos bancos são mantidos nos volumes Docker definidos no docker-compose.yml.

Caso queira apagar e remover os dados:

```text
docker compose down -v
```

---

## 🎯 Objetivo e aprendizados

O principal objetivo do projeto foi colocar em prática conceitos estudados sobre **arquitetura de microsserviços e mensageria**, indo além de uma aplicação monolítica tradicional.

Durante o desenvolvimento foram explorados conceitos como:

* Comunicação entre microsserviços
* Comunicação assíncrona
* RabbitMQ
* Exchanges e filas
* Eventos e mensagens
* Saga Orchestrated
* Transações distribuídas
* Operações de compensação
* Separação de responsabilidades
* Database per Service
* Containers com Docker

O projeto serviu como uma forma prática de compreender os desafios envolvidos na construção e comunicação de sistemas distribuídos.

---

## 📌 Status

🚧 Projeto desenvolvido para fins de estudo e aplicação prática dos conceitos de **microsserviços e mensageria**.
