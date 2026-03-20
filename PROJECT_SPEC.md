# PROJECT_SPEC

## Obiettivi

- Fornire una piccola utility Windows che vive nell'area di notifica.
- Mostrare rapidamente informazioni di sistema utili all'utente locale.
- Consentire l'avvio rapido di Assistenza rapida.
- Consentire la visualizzazione delle stampanti installate e l'impostazione della stampante predefinita.

## Architettura

- Applicazione WinForms .NET Framework 4.0.
- Un singolo progetto C# in `src/OnlyTrayInfo`.
- `Program.cs` contiene bootstrap applicativo, form principale, raccolta dati di sistema e logica UI.
- Gli script PowerShell in `scripts/` gestiscono build, pulizia e verifica degli output generati.

## Comportamento Atteso

- All'avvio l'applicazione parte minimizzata nel tray.
- L'icona tray espone un tooltip con hostname e IPv4 principale.
- La finestra principale mostra informazioni di sistema, rete, stampanti e log errori.
- L'utente puo' riaprire la finestra dal tray, avviare Assistenza rapida e impostare una stampante predefinita.

## Vincoli

- Progetto destinato a Windows.
- Dipendenza da WinForms, API di rete Windows e API di stampa Windows.
- Target framework impostato a .NET Framework 4.0.
- Lo stato del repository deve rimanere coerente tra codice, documentazione e `PROJECT_STATUS.json`.
