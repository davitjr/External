using System.Collections.Generic;
using System.Linq;

namespace ExternalBanking
{
    public class AutomateCardBlockingUnblocking
    {
        public readonly static int[] FreezingReasonsForBlocking = { 4, 5, 6, 25, 26, 27, 28 };

        public static ArcaCardsTransationActionReason GetCardTransactionReasonByFreezeReasonId(ushort freezeReason)
        {
            return (ArcaCardsTransationActionReason)Info.GetCardTransactionReasonByFreezeReasonId(freezeReason);
        }

        public static List<string> GetAllCardNumbers(Card card, ulong customerNumber)
        {
            List<Card> cards = Card.GetAttachedCards((ulong)card.ProductId, customerNumber);
            cards.Add(card);
            List<string> cardNumbers = cards.Select(m => m.CardNumber).ToList();
            List<PlasticCard> plasticCards = PlasticCard.GetSupplementaryPlasticCards(card.CardNumber);
            cardNumbers.AddRange(plasticCards.Select(m => m.CardNumber).ToList());
            return cardNumbers;
        }
    }
}
