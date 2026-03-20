# OnlyTrayInfo

OnlyTrayInfo e' una piccola applicazione WinForms che vive nell'area di notifica di Windows e mostra informazioni rapide sul PC (rete, stampanti, utente, ecc.). L'interfaccia principale offre un riepilogo dettagliato e alcune azioni rapide come l'avvio di **Assistenza rapida** e l'impostazione della stampante predefinita.

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

## Setup

1. Clona il repository su una macchina Windows.
2. Apri PowerShell nella root del repository.
3. Usa gli script in `scripts/` oppure Visual Studio/MSBuild per compilare il progetto.

## Struttura del progetto

```
src/
  OnlyTrayInfo/
    Program.cs         # UI, logica principali e raccolta info di sistema
    Properties/
      AssemblyInfo.cs  # metadati assembly
    OnlyTrayInfo.csproj  # progetto .NET Framework 4.0
```

## Build

Il workflow del repository usa come output comune la cartella root `build/`:

- `build/Debug/` per build Debug eseguite tramite progetto/MSBuild
- `build/Release/` per build Release
- `tmp/` per file temporanei di compilazione rimossi dagli script

### Script PowerShell del repository
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

Lo script genera `build\Release\OnlyTrayInfo.exe` e aggiorna la versione informativa con timestamp.

Per pulire gli output generati:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\clean.ps1
```

Per eseguire una verifica minima ripetibile:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1
```

Lo script esegue la build e verifica che `build\Release\OnlyTrayInfo.exe` esista
e riporti metadata coerenti con il nome prodotto.

### Visual Studio
1. Apri `src/OnlyTrayInfo/OnlyTrayInfo.csproj`.
2. Seleziona **Build > Build Solution**.
3. Recupera gli output in `build\Debug\` o `build\Release\` in base alla configurazione scelta.

### MSBuild (Developer Command Prompt)
```bat
cd src\OnlyTrayInfo
msbuild OnlyTrayInfo.csproj /p:Configuration=Release
```

## Test

Non e' presente una suite di test automatizzati. La verifica ripetibile disponibile e' lo smoke check:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1
```

Lo script esegue `clean`, `build` e controlla che l'eseguibile finale esponga metadata coerenti.

## Esecuzione

1. Avvia `OnlyTrayInfo.exe` generato in `build\Release\`.
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

## Clean

Per riportare il repository a uno stato sorgente-only compatibile con i file versionati:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\clean.ps1
```

Lo script rimuove `build/`, `tmp/` e gli eventuali artefatti `bin/` e `obj/`.

## Publish

Il repository non definisce al momento uno script o un flusso separato di packaging/publish.

## Struttura essenziale

```text
/
├── scripts/
│   ├── build.ps1
│   ├── clean.ps1
│   └── verify.ps1
└── src/
    └── OnlyTrayInfo/
        ├── OnlyTrayInfo.csproj
        ├── Program.cs
        ├── app.manifest
        └── Properties/AssemblyInfo.cs
```

## Note

- L’applicazione usa un **log interno in memoria** per gli errori più comuni, visibile nella sezione “LOG ERRORI”.
- Il tooltip del tray è limitato a 63 caratteri (limite standard di Windows).

## Licenza

Vedi [LICENSE](LICENSE).

## Copyright

Copyright (c) 2026 Danny Perondi. All rights reserved.

Questo progetto e' proprietario e confidenziale. La consultazione del
repository e' consentita solo per visione e valutazione da parte di soggetti
autorizzati.

Sono vietati senza preventiva autorizzazione scritta di Danny Perondi il
riutilizzo, la copia, la modifica, la distribuzione, la sublicenza e qualsiasi
uso commerciale, totale o parziale.
