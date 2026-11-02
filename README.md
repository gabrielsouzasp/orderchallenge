# Order Challenge — OrderGenerator & OrderAccumulator

Duas aplicações C# que se comunicam via protocolo FIX 4.4: o **OrderGenerator** envia ordens (NewOrderSingle) a partir de um formulário web e o **OrderAccumulator** calcula a exposição financeira por símbolo, aceitando ou rejeitando cada ordem conforme o limite de R$ 100.000.000.

>  This is a challenge by [Coodesh](https://coodesh.com/)

## Tecnologias

- C# / .NET 10
- ASP.NET Core Razor Pages (frontend do OrderGenerator)
- Bootstrap 5
- [QuickFIX/n](https://quickfixn.org/) 1.14.1 (FIX 4.4)
- xUnit (testes)
- Docker / Docker Compose

## Estrutura

```
OrderChallenge.slnx
src/
  OrderGenerator.Web/   # App web + FIX initiator (CLIENT1)
  OrderAccumulator/     # Console + FIX acceptor (BROKER), porta 9876
tests/
  OrderChallenge.Tests/ # Testes do OrderAccumulator
```

## Como instalar e usar

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) ou [Docker](https://www.docker.com/)

### Docker

Na raiz do repositório:

```bash
docker compose up --build
```

- OrderGenerator: http://localhost:5121
- OrderAccumulator: container `orderaccumulator`, porta FIX 9876 (rede interna do compose)

Para encerrar:

```bash
docker compose down
```
