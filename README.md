# HandmadeDollsAPI
![EndPoints](https://github.com/igormor4es/HandmadeDollsAPI/blob/main/ImgEndPoints.PNG)

A HandmadeDollsAPI foi criada na Arquietetura de MinimalAPI, permitindo criar APIs com o mínimo de dependência do framework WebAPI e o mínimo de código e arquivos necessários para o desenvolvimento minimalista de APIs. Como principais objetivos as Minimals APIs da plataforma .NET permitem :

- Reduzir a complexidade do código para quem esta começando agora;
- Reduzir recursos não essenciais (controllers, roteamento, filtros, etc) que estavam presentes nos projetos ASP.NET;
- Abraçar o conceito de minimalismo a nível de API;
- Ser Escalável, garantido que sua aplicação possa crescer se necessário;

Como a API não iria ter muitos EndPoints, é uma ótima solução para implementação de microsserviços e aplicativos que consumam o mínimo de dependências do ASP .NET Core, por exemplo. Ficamos então com o lema minimalista “Menos é mais”. 😉

## Tecnologias
* C# ASP .NET CORE 6
* ORM Entity Framework
* SQL SERVER
* SWAGGER: API Documentation & Design Tools

## Regras de Negócio
API de uma Loja de Bonecas, seguindo as seguintes regras de negócio:
- Ana Katarina possui uma lojinha que vende bonecas de crochê, feitas à mão por ela, e vários acessórios para suas bonecas. Algumas bonecas vendidas, vêm, gratuitamente, acompanhadas de alguns acessórios: pulseirinhas, roupinhas etc. Estes mesmos acessórios podem ser vendidos separadamente.
- Atualmente a lojinha funciona somente de forma presencial e ela gostaria de começar a vender online.

Ana Katarina, então, contratou você para criar uma API REST onde seja possível encomendar seus produtos. O sistema servirá apenas para realizar a encomendas de seus produtos.

Nessa API será preciso: 

- Cadastrar os produtos, bonecas e acessórios, que estão à venda.
- Listar todos os produtos disponíveis.
- Visualizar um produto específico. Se esse produto for uma boneca que acompanha acessórios, deve ser apresentado os acessórios que vêm com a boneca.
- Geração do pedido. Indicando quais produtos serão encomendados.
- Listagem de todos os pedidos.
- Documento (read.md) identificando os endpoints.

Extras: Teste Unitários, Autenticação de usuários, associação entre pedidos e usuários, Docker

## Instalação
Passo-a-passo para instalação e configuração do ambiente de desenvolvimento.

Clone o repositório em sua máquina:
```
git clone https://github.com/igormor4es/HandmadeDollsAPI.git
```
A API conta com três contextos de aplicação, porém, precisamos executar os comandos somente em dois deles:
```
Add-Migration -v Initial -Context MinimalContextDb
Add-Migration -v AuthInitial -Context NetDevPackAppDbContext

Update-Database -v -Context MinimalContextDb
Update-Database -v -Context NetDevPackAppDbContext
```
Instalação Visual Studio 2022
```
https://visualstudio.microsoft.com/pt-br/vs/
```

## EndPoints
Para utilizar os EndPoints, deve-se estar autorizado na API. Caso não tenha um Usuário, somente fazer no cadastro e pegar no retorno o JWT, clicar no botão superior direito com nome de Authorize e inserir o Token conforme orientação descrita.
Algumas ações demandam que o usuário tenha uma Claim específica. Basta cadastrar a claim no endpoint /UserClaim. Na API trabalhamos com duas Claims, Administrator ou Customer.

Importante ressaltar que para os Status da Order e ProductType do Product trabalhei com ENUM's, a API envia a String, porém, o banco salva como Int. Irei anexar aqui os ENUM's caso queria enviar algum outro tipo de Status pra Order, pois foi pensando caso futuramente ela sofra alterações, basta somenter criar outro EndPoint para alterarção desse Status.

```
public enum ProductType
{
    DOLL = 1,
    ACCESSORY = 2
}

public enum OrderStatus
{
    ORDER_RECEIVED = 1,
    AWAITING_PAYMENT = 2,
    ORDER_IN_SEPARATION = 3,
    INVOICE_ISSUED = 4,
    ORDER_DELIVERED = 5
}
```

### Order
- /Order ==> GET
- /Order ==> POST
```
Exemplo:
{
  "orderStatus": "ORDER_RECEIVED",
  "orderLists": [
    {
      "quantity": 1,
      "productId": 2
    }
  ]
}
```
- /Order/{id} ==> GET
### Product
- /Product ==> GET
- /Produt ==> POST
```
Exemplo POST de Acessório:
{
  "description": "Laço de Cabelo",
  "price": 9.99,
  "stock": 1,
  "image": null,
  "active": true,
  "productType": "ACCESSORY"
}

Exemplo POST de Boneca:
{
  "description": "Boneca de Crochê",
  "price": 199.99,
  "stock": 1,
  "image": null,
  "active": true,
  "productType": "DOLL",
  "accessories": [
    {
      "accessoryId": 1
    }
  ]
}
```
- /Product/{id} ==> GET
- /Product/{id} ==> PUT 
```
Exemplo PUT de Acessório:
{
  "id": 1
  "description": "Laço de Cabelo",
  "price": 19.99,
  "stock": 1,
  "image": null,
  "active": true,
  "productType": "ACCESSORY"
}
```
### User
- /Register ==> POST
```
Exemplo Register:
{
  "email": "teste@teste.com.br",
  "password": "Teste@1234",
  "confirmPassword": "Teste@1234"
}
```
- /Login ==> POST
```
Exemplo Login:
{
  "email": "teste@teste.com.br",
  "password": "Teste@1234"
}
```
- /UserClaim ==> POST
```
Login User: Email Cadastrado
Tipos de Claims: Administrator ou Customer
```
- /UserClaim ==> DELETE
```
Mesmas informções do POST /UserClaim
```
