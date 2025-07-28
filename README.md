# JitHook

JitHook é um conjunto de bibliotecas experimentais para interceptar a fase de JIT (Just-In-Time) do CLR e permitir monitoramento ou alterações dinâmicas em métodos .NET. O projeto demonstra como substituir o método `CompileMethod` do JIT para injetar código intermediário (IL) e despachar chamadas para monitores definidos pelo usuário.

## Estrutura do repositório

- **JitHook.Agent.Core** – Interfaces nativas e definições compartilhadas.
- **JitHook.Agent.Hook** – Implementação do hook que troca o ponteiro do `CompileMethod`.
- **JitHook.Agent.Runtime** – Gera código IL em tempo de execução e encaminha para monitores.
- **JitHook.Agent.Test** – Aplicação de exemplo que demonstra o uso da biblioteca.
- **NetFramework** – Versão equivalente para .NET Framework.

## Requisitos

- .NET Core 3.1 (ou superior) para os projetos principais.
- Windows ou macOS para os exemplos de código nativo. A implementação para Linux não está incluída.

## Como compilar

Execute:

```bash
dotnet build
```

Para rodar o projeto de demonstração:

```bash
dotnet run --project JitHook.Agent.Test
```

O exemplo cria um `JitHook`, registra um `RuntimeDispatcher` e aplica filtros para interceptar métodos da biblioteca `MockLibrary`.

## Exemplo rápido

Trecho simplificado de `Program.cs` no projeto de teste:

```csharp
var dispatcher = new RuntimeDispatcher(Headers.Platform.WIN);
var hook = new JitHook(Headers.Platform.WIN);
hook.InstallHook(new HookDelegate(dispatcher.CompileMethod));

dispatcher.AddFilter(typeof(TestMethodMonitor), "MockLibrary.StaticClass.Method1");

hook.Start();
StaticClass.Method1();
hook.Stop();
```

Esse código intercepta a compilação de `StaticClass.Method1` e executa o monitor `TestMethodMonitor` quando o método é chamado.

## Aviso

Este projeto possui fins educativos e demonstração de técnicas de hooking no CLR. A API está sujeita a mudanças e não é recomendada para uso em produção sem revisão adicional.
