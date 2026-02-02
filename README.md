# TrayPcInfo

TrayPcInfo è una piccola applicazione WinForms che vive nell’area di notifica di Windows e mostra informazioni rapide sul PC (rete, stampanti, utente, ecc.). L’interfaccia principale offre un riepilogo dettagliato e alcune azioni rapide come l’avvio di **Assistenza rapida** e l’impostazione della stampante predefinita.

## Funzionalità principali

- **Tray icon** con tooltip dinamico (hostname + IP principale).
- **Dashboard di sistema** con:
  - hostname, utente e dominio DNS;
  - elenco delle interfacce di rete con stato, velocità, MAC, IPv4, gateway e DNS;
  - elenco delle stampanti installate con indicazione di quella predefinita;
  - log errori interno.
- **Assistenza rapida**: apertura di Quick Assist tramite protocollo `ms-quick-assist:` o eseguibile.
- **Gestione stampanti**: selezione rapida e impostazione stampante predefinita.
- **Avvio in tray**: la finestra resta nascosta e si riapre dal menu del tray.

## Requisiti

- **Windows** con supporto a WinForms.
- **.NET Framework 4.0** (target del progetto).
- Accesso alle API di rete e stampa di Windows (per la lettura di NIC e stampanti).

## Struttura del progetto

```
src/
  TrayPcInfo/
    Program.cs         # UI, logica principali e raccolta info di sistema
    Properties/
      AssemblyInfo.cs  # metadati assembly
    TrayPcInfo.csproj  # progetto .NET Framework 4.0
```

## Build

Il progetto è un’app WinForms .NET Framework. Puoi compilarlo con:

### Visual Studio
1. Apri `src/TrayPcInfo/TrayPcInfo.csproj`.
2. Seleziona **Build > Build Solution**.

### MSBuild (Developer Command Prompt)
```bat
cd src\TrayPcInfo
msbuild TrayPcInfo.csproj /p:Configuration=Release
```

## Esecuzione

1. Avvia `TrayPcInfo.exe` generato in `bin\Release\`.
2. L’app parte **minimizzata nel tray**.
3. Doppio click o menu contestuale ➜ **Apri** per mostrare la finestra.
4. **Esci** dal menu tray per chiudere l’app.

## Uso dell’interfaccia

### Barra strumenti
- **Assistenza rapida**: avvia Quick Assist per supporto remoto.

### Sezione stampanti
- **Stampanti**: seleziona la stampante dalla lista.
- **Imposta predefinita**: la imposta come stampante predefinita di sistema.

### Pannello informazioni
Mostra un riepilogo testuale con:
- dati di sistema;
- dati rete (una sezione per ogni NIC attiva);
- elenco stampanti;
- log errori (utile per diagnosi locali).

## Risoluzione problemi

### L’app non mostra l’IP o la rete
Verifica che l’interfaccia di rete sia **Attiva**. L’app filtra le NIC **Loopback** e **Tunnel**.

### Non si riesce a impostare la stampante predefinita
Assicurati che:
- la stampante sia installata correttamente;
- l’app abbia i permessi necessari;
- la stampante sia selezionata nella lista.

### Quick Assist non si apre
L’app prova prima il protocollo `ms-quick-assist:` e poi cerca `quickassist.exe` in:
- `%WINDIR%\System32`
- `%WINDIR%\SysWOW64`
- `PATH`

Se non è presente, installa/abilita **Assistenza rapida** in Windows.

## Note

- L’applicazione usa un **log interno in memoria** per gli errori più comuni, visibile nella sezione “LOG ERRORI”.
- Il tooltip del tray è limitato a 63 caratteri (limite standard di Windows).

## Licenza

Vedi [LICENSE](LICENSE).
