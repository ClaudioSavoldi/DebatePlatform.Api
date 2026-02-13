Debate Platform – Backend API

Backend RESTful per una piattaforma di dibattiti strutturati 1vs1 con sistema di moderazione, matchmaking automatico, fasi argomentative e votazione pubblica.
Il progetto è sviluppato con ASP.NET Core Web API, Entity Framework Core, SQL Server e autenticazione JWT tramite ASP.NET Identity.

Tecnologie utilizzate
ASP.NET Core Web API
Entity Framework Core
SQL Server
ASP.NET Identity
JWT Authentication
Swagger / OpenAPI
C#

Descrizione del Progetto
La piattaforma consente agli utenti di:
Creare topic di dibattito
Iscriversi scegliendo un lato (Pro / Contro)
Essere abbinati automaticamente a un avversario
Scrivere una Opening
Scrivere una Rebuttal
Sottoporsi a votazione pubblica
Visualizzare risultati e vincitore

Il sistema prevede inoltre:
Moderazione dei topic
Gestione dello stato dei dibattiti
Matchmaking FIFO
Gestione coda di iscrizione
Calcolo vincitore


Architettura Logica

1. Debate
Rappresenta il topic generale.
Stati possibili:

Open
InReview
Approved
Rejected
Closed

Solo i dibattiti Approved sono visibili pubblicamente.
È presente una tabella DebateStatusHistory che traccia ogni transizione di stato.

2. Sistema di Coda (Matchmaking)
Quando un utente si iscrive a un dibattito:

Viene inserito in DebateQueueEntries
Se esiste un utente sul lato opposto, viene creato un DebateMatch
Entrambi vengono rimossi dalla coda
Matchmaking FIFO basato su JoinedAt.

Endpoint principali:
POST /api/debates/{debateId}/join
GET /api/debates/queue/mine

3. Match (DebateMatch)
Rappresenta uno scontro 1vs1 tra due utenti.

Fasi:

Opening
Rebuttal
Voting
Closed

Il match avanza automaticamente quando:

Entrambe le Opening sono consegnate
Entrambi i Rebuttal sono consegnati
Alla fine del Rebuttal:
Phase = Voting
VotingEndsAt = DateTime.UtcNow + durata configurata
Chiusura automatica in modalità lazy-close al primo accesso dopo scadenza.

4. Submission
Entità: MatchSubmission
Ogni fase ha:

Draft (modificabile)
Submit (definitivo)
IsSubmitted
SubmittedAt
UpdatedAt

Endpoint:

PUT /api/matches/{matchId}/submissions/opening/draft
POST /api/matches/{matchId}/submissions/opening/submit
PUT /api/matches/{matchId}/submissions/rebuttal/draft
POST /api/matches/{matchId}/submissions/rebuttal/submit

Le submission diventano pubbliche solo quando entrambi hanno consegnato.

5. Votazione
Durante la fase Voting:

Gli utenti pubblici possono votare Pro o Contro
Un utente può votare una sola volta per match
I partecipanti al match non possono votare
Il vincitore viene calcolato in base ai voti:

Maggioranza Pro → vince ProUser

Maggioranza Contro → vince ControUser

Parità → IsDraw = true

Endpoint:
POST /api/matches/{matchId}/votes
GET /api/matches/public
GET /api/matches/results

6. Autenticazione e Autorizzazione
Sistema basato su:

ASP.NET Identity
JWT Bearer Authentication
Role-based Authorization

Ruoli:
User
Moderator

I Moderatori possono:

Visualizzare dibattiti in moderazione
Cambiare stato dei topic

Endpoint:
POST /api/auth/register
POST /api/auth/login
GET /api/debates/moderation
POST /api/debates/{id}/status


Considerazioni Architetturali

Separazione tra dominio e Identity
DTO separati dalle entità
Matchmaking transazionale
Lazy close per evitare background services
Nessuna logica sensibile lato frontend
Sicurezza garantita da controlli backend su ruoli e ownership



