namespace DebatePlatform.Api.Domain.Enums
{
    public enum DebateStatus
    {
        Open = 1,       // creato dall'utente
        InReview = 2,   // preso in carico dal moderatore
        Approved = 3,   // accettato
        Rejected = 4,   // rifiutato
        Closed = 5      // chiuso (eventuale dopo voto / fine)
    }
}