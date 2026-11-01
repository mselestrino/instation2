# Instation 2

Uma biblioteca e launcher de jogos de PlayStation 2 para Windows, com interface inspirada no XMB do PlayStation 3.

## Estado do projeto

A primeira base visual está em desenvolvimento. O protótipo atual abre em tela cheia, apresenta o fundo animado e permite navegar pelas categorias e opções com o teclado.

## Requisitos

- Windows 10 ou 11
- .NET 8 SDK

## Executar em desenvolvimento

Abra um terminal na pasta `Instation2` e execute:

```powershell
dotnet restore
dotnet run
```

## Controles

- **← / →**: mudar categoria
- **↑ / ↓**: navegar
- **Enter**: selecionar
- **Esc**: sair

## Próximas etapas

1. Biblioteca de jogos e leitura de capas/metadados.
2. Cadastro e verificação de pastas com jogos.
3. Integração com PCSX2 e inicialização dos jogos.
4. Configurações, favoritos e histórico.

O projeto não distribui imagens ISO comerciais. Use cópias de jogos que você tenha direito de utilizar.
