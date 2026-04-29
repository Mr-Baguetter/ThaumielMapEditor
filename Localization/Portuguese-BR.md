# Thaumiel Map Editor (``TME``)

> [!WARNING]
> **Este projeto atualmente se encontra em fase de desenvolvimento. Espere bugs, funcionalidades faltando ou até mesmo mudanças que quebram compatibilidade.**

Thaumiel Map Editor é um editor de mapa baseado na Unity voltado a criação, edição e manutenção de mapas para o **SCP:SL**.

O ``TME`` dispõe de uma interface de dentro do editor para colocar e configurar uma variedade de ``GameObjects``, de elementos interativos e luzes até props.

---

<table align="center" style="width: 100%; max-width: 600px; border-collapse: separate; border-spacing: 15px;">
    <tr>
        <td align="center" style="background-color: #1d1d1d; border-radius: 10px; padding: 10px; ; width: 100px;">
            <a href="README.md" 
               style="display: block; width: 100%; height: 100%; text-align: center; text-decoration: none; color: #333; cursor: pointer;">
                <img src="https://flagsapi.com/US/flat/64.png" height=30><br>
                <span style="color: #f0f0f0">English</span>
            </a>
        </td>
        <td align="center" style="background-color: #1d1d1d; border-radius: 10px; padding: 10px; width: 100px;">
            <a href="Localization/Russian.md" 
               style="display: block; width: 100%; height: 100%; text-align: center; text-decoration: none; color: #333; cursor: pointer;">
                <img src="https://flagsapi.com/RU/flat/64.png" height=30><br>
                <span style="color: #f0f0f0">Русский</span>
            </a>
        </td>
        <td align="center" style="background-color: #1d1d1d; border-radius: 10px; padding: 10px; width: 100px;">
            <a href="Localization/Spanish.md" 
               style="display: block; width: 100%; height: 100%; text-align: center; text-decoration: none; color: #333; cursor: pointer;">
                <img src="https://flagsapi.com/ES/flat/64.png" height=30><br>
                <span style="color: #f0f0f0">Español</span>
            </a>
        </td>
        <td align="center" style="background-color: #1d1d1d; border-radius: 10px; padding: 10px; width: 100px;">
            <a href="Localization/French.md" 
               style="display: block; width: 100%; height: 100%; text-align: center; text-decoration: none; color: #333; cursor: pointer;">
                <img src="https://flagsapi.com/FR/flat/64.png" height=30><br>
                <span style="color: #f0f0f0">Français</span>
            </a>
        </td>
        <td align="center" style="background-color: #1d1d1d; border-radius: 10px; padding: 10px; width: 100px;">
            <a href="Localization/Portuguese-BR.md" 
               style="display: block; width: 100%; height: 100%; text-align: center; text-decoration: none; color: #333; cursor: pointer;">
                <img src="https://flagsapi.com/BR/flat/64.png" height=30><br>
                <span style="color: #f0f0f0">Português-BR</span>
            </a>
        </td>
    </tr>
</table>

---

## Funcionalidades

> Novas implementações estão sendo ativamente desenvolvidas. A lista abaixo reflete apenas o que já foi adicionado.

- **Interface de Edição Construida na Unity** - Edite e crie seu mapa por meio da Unity
- **Sistema de Criação de Objetos** — Coloque uma vasta gama de objetos por meio de paines da Unity
- **Suporte a Objetos ``Client-Side``** — Alguns objetos são apenas criados no cliente, resultando em ganhos de perfomance no servidor
- **Salvamento e Carregamento de Mapas** — Faça seu mapa ser persistente de dentro do Servidor

---

## ``GameObjects`` suportados

> [!NOTE]
>  Objetos marcados como ``Client`` são criados apenas no cliente e não são replicados no Servidor.

O ``TME`` suporta o spawn dos seguintes objetos:

| Tipo | Escopo |
|-|-|
| Portas | ``Server`` |
| Clutters | ``Server`` |
| Interativos | ``Server`` |
| Pickups | ``Server`` |
| Armários | ``Server`` |
| Waypoints | ``Server`` |
| Cameras | ``Server`` |
| Targets | ``Server`` |
| Teletransportadores | ``Server`` |
| Primitivos | ``Client`` |
| Luzes | ``Client`` |
| Capibaras | ``Client`` |

---
## Uso & Comandos

Para ver a lista completa de comandos, permissões, e apelidos de comandos, por favor cheque nossa **[Documentação de Comandos](Commands-PT-BR.md)**.

## Instalação

> [!IMPORTANT]
> Antes de começar, garanta que você tem o [Unity Hub](https://unity.com/download) instalado.


1. Baixe o repositório inteiro aqui: https://www.github.com/Mr-Baguetter/ThaumielMapEditorUnityProject

2. Extraia os arquivos.

3. Abra o projeto no [Unity Hub](https://unity.com/download) clicando em ``Add`` e selecionando a pasta extraída.

---

Nosso Discord: https://discord.gg/N8qrNHf4s9

Aviso de dependências: [Dependencies.md](Dependencies.md)

*O Thaumiel Map Editor é uma "obra em andamento". Contribuições, feeebacks, e aviso sobre erros ou problemas são bem-vindos.*
*Traduzido por [Unbistrackted](https://github.com/Unbistrackted).**
