# 🛒 Sales

Sistema de vendas desenvolvido com **C# e .NET**, utilizando uma arquitetura baseada em **microsserviços**, **RabbitMQ** e comunicação assíncrona entre os serviços.

O projeto foi desenvolvido com o objetivo de aplicar na prática conceitos de **arquitetura de microsserviços, mensageria e processamento assíncrono**, explorando a comunicação entre diferentes serviços e o gerenciamento de uma transação distribuída utilizando o padrão **Saga Orchestrated**.

---

## 🏗️ Arquitetura

O sistema é composto por três microsserviços:

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

## 🔀 Fluxo do processamento

> 🚧 **Em construção**

O fluxo detalhado do processamento de um pedido será apresentado nesta seção, incluindo:

1. Criação do pedido
2. Comunicação com o Inventory Service
3. Reserva do estoque
4. Comunicação com o Payment Service
5. Processamento do pagamento
6. Atualização do status do pedido
7. Tratamento de falhas
8. Operações de compensação da Saga

---

## 🐳 Executando o projeto

> 🚧 **Em construção**

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
