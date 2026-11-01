# Instation 2

Uma biblioteca e launcher de jogos de PlayStation 2 para Windows, com interface inspirada no XMB do PlayStation 3.

## Estado do projeto

A versão atual já possui leitura de jogos em pastas cadastradas, salvamento das configurações, detecção de capas locais e integração inicial com o PCSX2.

## Requisitos

- Windows 10 ou 11
- .NET 8 SDK para executar em desenvolvimento

## Executar em desenvolvimento

Abra um terminal na pasta `Instation2` e execute:

```powershell
dotnet restore
dotnet run
```

## Biblioteca de jogos

No programa, use **Adicionar pasta** para selecionar uma ou mais pastas. O Instation 2 procura arquivos:

- ISO, CHD, CSO, BIN, GZ e ELF

As pastas cadastradas são salvas em `%LOCALAPPDATA%\Instation2\settings.json`.

### Capas locais

Para mostrar a capa de um jogo, coloque uma imagem JPG, JPEG, PNG ou WEBP com o mesmo nome do arquivo do jogo.

Exemplo:

```text
Jogos PS2/
  Gran Turismo 4.iso
  Gran Turismo 4.jpg
```

Também é possível guardar as capas em uma subpasta chamada `covers`:

```text
Jogos PS2/
  Gran Turismo 4.iso
  covers/
    Gran Turismo 4.jpg
```

## PCSX2

Use o botão **Emulador** para selecionar `pcsx2-qt.exe`. Depois selecione um jogo e pressione **Enter** para iniciá-lo.

## Controles

- **← / →**: mudar categoria
- **↑ / ↓**: navegar pelos jogos
- **Enter**: iniciar jogo selecionado
- **A**: adicionar pasta
- **F5**: verificar biblioteca novamente
- **Esc**: sair

## Próximas etapas

1. Catálogo de capas e informações por título.
2. Favoritos e histórico de jogos.
3. Refinar a navegação XMB e a apresentação visual da biblioteca.
4. Melhorar a integração com o PCSX2.

O projeto não distribui imagens ISO comerciais. Use cópias de jogos que você tenha direito de utilizar.


## Catálogo online de metadados

O launcher consulta `catalog/games.json` no repositório para associar nomes de arquivos a títulos, códigos de produto, região, descrição e capas. Os resultados são armazenados em cache em `%LOCALAPPDATA%\\Instation2\\cache`, permitindo reutilizar o último catálogo disponível quando estiver sem internet.

A correspondência usa o nome do arquivo do jogo (sem extensão) e os aliases cadastrados. Para adicionar uma entrada, use este formato:

```json
[
  {
    "title": "Nome oficial do jogo",
    "aliases": ["Nome alternativo usado no arquivo"],
    "serial": "Código do disco, se conhecido",
    "region": "Região, se conhecida",
    "description": "Descrição curta",
    "coverUrl": "https:// endereço de uma capa com uso autorizado"
  }
]
```

O catálogo inicial está vazio de propósito: somente dados verificados e imagens com origem/licença adequada devem ser adicionados. Capas locais continuam tendo prioridade sobre as capas do catálogo.
