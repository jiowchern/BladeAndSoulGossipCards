﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BladeAndSoulGossipCards
{
    class CardSet : Regulus.Utility.Singleton<CardSet>
    {
        SetEffect[] _Effects;
        Card[] _Cards;
        internal Card[] Find(Property[] propertys)
        {
            Card[] suitCards = _FindPropertyInSuit(propertys);
            List<Card> cards = _FindProperty(propertys);
            

            return suitCards.Union(cards).ToArray();
        }

        private List<Card> _FindProperty(Property[] propertys)
        {
            List<Card> cards = new List<Card>();
            foreach (var property in propertys)
            {
                for(int no = 1 ; no <= 8 ; ++no)
                {
                    var c = (from card in _Cards
                             let val = card.GetValue(property)                             
                             where card.No == no 
                             orderby val descending
                             select card).FirstOrDefault();                    
                    cards.Add(c);
                }
                
            }
            return cards;
        }

        private Card[] _FindPropertyInSuit(Property[] propertys)
        {
            Card[] totalCards = new Card[0];
            foreach (var property in propertys)
            {
                List<Card> cards = new List<Card>();
                var suits = from e in _Effects
                           where e.GetValue(property) > 0
                           select e.Id;
                foreach(var suit in suits)
                {
                    for(int no = 1 ; no <= 8 ; ++ no)
                    {
                        var card = (from c in _Cards
                                    let val = c.GetValue(property)
                                    orderby val descending
                                    where c.Group == suit && c.No == no
                                    select c).FirstOrDefault();

                        cards.Add(card);
                    }
                }
                totalCards = totalCards.Union(cards).ToArray();
            }
            return totalCards;
        }

        private bool _HasProperty(Card card, Property[] propertys)
        {
            foreach(var property in propertys)
            {                
                if (card.HasProperty(property))
                    return true;
            }
            return false;
        }

        internal Card[] Find(int no)
        {
            return (from card in _Cards where card.No == no select card).ToArray();
        }



        internal int GetValue(Property property, string group_id, int count)
        {
            return (from effect in _Effects where effect.Id == group_id && count >= effect.Count select effect.GetValue(property)).Sum();
        }

        internal bool HasProperty(string id, Property property)
        {
            return (from effect in _Effects where effect.Id == id select effect.GetValue(property)).Sum() > 0;
        }

        internal void Load(string path)
        {
            var text = System.IO.File.ReadAllText(path);
            var cardsuits = Newtonsoft.Json.JsonConvert.DeserializeObject<CardSuit[]>(text);

            _Build(cardsuits);
        }

        private void _Build(CardSuit[] cardsuits)
        {
            List<Card> cards = new List<Card>();
            List<SetEffect> setEffects = new List<SetEffect>();
            foreach(var suit in cardsuits)
            {
                foreach(var e in suit.Effects)
                {
                    var setEffect = new SetEffect(e.Propertys) { Id = suit.Name, Count = e.Count };
                    setEffects.Add(setEffect);
                }
                
                foreach(var c in  suit.Cards)
                {
                    var card = new Card(c.Propertys) { No = c.No, Group = suit.Name };
                    cards.Add(card);
                }

                _Cards = cards.ToArray();
                _Effects = setEffects.ToArray();
            }
        }

        internal IEnumerable<string> GetSuitNames()
        {
            foreach(var effect in from e in  _Effects group e by e.Id into g select g)
            {
                yield return effect.Key;
            }            
        }
    }
}
