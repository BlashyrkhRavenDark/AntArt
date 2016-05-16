using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;

namespace WindowsFormsApplication1
{
    class AAMTGLibrary
    {
        public Hashtable m_oMtgSets;

        public AAMTGLibrary(string p_sLibFile)
        {
            m_oMtgSets = new Hashtable();
            JObject aMtgSets = JObject.Parse(File.ReadAllText(p_sLibFile));

            // get JSON result objects into a list
            IList<JToken> jMtgSets = aMtgSets.Children().ToList();
            foreach (JToken jMtgSet in jMtgSets)
            {
                // Creates AAMTGSet class from JSON data
                AAMTGSet oSet = JsonConvert.DeserializeObject<AAMTGSet>(jMtgSet.First.ToString());
                // Adds link pointing to parent AAMTGSet to every AAMTGCard in a set.
                oSet.AddParentSetToCards();
                m_oMtgSets.Add(oSet.name, oSet);
            }
        }

        public AAMTGSet GetSet(string p_sSetName)
        {
            if (m_oMtgSets.ContainsKey(p_sSetName))
                return (AAMTGSet)m_oMtgSets[p_sSetName];
            return null;
        }

        public AAMTGCard GetCard(string p_sSetName, string p_sCardName)
        {
            AAMTGSet oSet = GetSet(p_sSetName);
            if (oSet != null)
                return oSet.GetCard(p_sCardName);
            return null;
        }
    }

    class AAMTGSet
    {
        #region JASON PARAMETERS
        public string name { get; set; }                        // The name of the set
        public string code { get; set; }                        // The set's abbreviated code
        public string gathererCode { get; set; }                // The code that Gatherer uses for the set. Only present if different than 'code'
        public string oldCode { get; set; }                     // An old style code used by some Magic software. Only present if different than 'gathererCode' and 'code'
        public string magicCardsInfoCode { get; set; }          //"ne",                 // The code that magiccards.info uses for the set. Only present if magiccards.info has this set
        public string releaseDate { get; set; }                 //"2000-02-14"          // When the set was released (YYYY-MM-DD). For promo sets, the date the first card was released.
        public string border { get; set; }                      //"black",              // The type of border on the cards, either "white", "black" or "silver"
        public string type { get; set; }                        // "expansion",          // Type of set. One of: "core", "expansion", "reprint", "box", "un",
                                                                //                      "from the vault", "premium deck", "duel deck",
                                                                //                      "starter", "commander", "planechase", "archenemy",
                                                                //                      "promo", "vanguard", "masters"
        public string block { get; set; }                       // "Masques",            // The block this set is in,
        public string onlineOnly { get; set; }                  // : false,                // Present and set to true if the set was only released online
        public List<AAMTGCard> cards { get; set; }              // ANT : The card list filled automatically by the JSON parser
        #endregion

        #region Our own Set values
        private ArrayList m_OrderedCards = null;                       // ANT : Our version of cards ordered if they match a specific set ("ie" for ice age, etc... )
        public ArrayList OrderedCards { get { return (m_OrderedCards == null ? ReorderSet() : m_OrderedCards); } }
        #endregion

        public AAMTGSet()
        {

        }

        public int AddParentSetToCards()
        {
            foreach (AAMTGCard oCard in cards)
            {
                // Adding parent set reference to card
                oCard.oParentSet = this;
            }
            return cards.Count;
        }


        /// <summary>
        /// Reorders the set according to the magiccards.info rule (Black cards then blue cards etc...)
        /// Is launched the first time the public member OrderedCards is accessed, any ulterior call to OrderedCards will return m_OrderedCards
        /// </summary>
        /// <returns> Returns m_OrderedCards after filling it</returns>
        public ArrayList ReorderSet()
        {
            ArrayList oListBlack = new ArrayList();
            ArrayList oListBlue = new ArrayList();
            ArrayList oListGreen = new ArrayList();
            ArrayList oListRed = new ArrayList();
            ArrayList oListWhite = new ArrayList();
            ArrayList oListArtefact = new ArrayList();
            ArrayList oListLands = new ArrayList();
            ArrayList oListMulti = new ArrayList();
            m_OrderedCards = new ArrayList();

            foreach (AAMTGCard oCard in cards)
            {

                if (oCard.bIsBlack && oCard.iColorNbr == 1)
                    oListBlack.Add(oCard);
                if (oCard.bIsBlue && oCard.iColorNbr == 1)
                    oListBlue.Add(oCard);
                if (oCard.bIsGreen && oCard.iColorNbr == 1)
                    oListGreen.Add(oCard);
                if (oCard.bIsRed && oCard.iColorNbr == 1)
                    oListRed.Add(oCard);
                if (oCard.bIsWhite && oCard.iColorNbr == 1)
                    oListWhite.Add(oCard);
                if (oCard.bIsArtefact)
                    oListArtefact.Add(oCard);
                if (oCard.bIsLand)
                    oListLands.Add(oCard);
                if (oCard.iColorNbr > 1)
                    oListMulti.Add(oCard);
            }

            int iCardNumber = 1;
            m_OrderedCards.AddRange(oListBlack);
            m_OrderedCards.AddRange(oListBlue);
            m_OrderedCards.AddRange(oListGreen);
            m_OrderedCards.AddRange(oListRed);
            m_OrderedCards.AddRange(oListWhite);
            m_OrderedCards.AddRange(oListArtefact);
            m_OrderedCards.AddRange(oListLands);
            m_OrderedCards.AddRange(oListMulti);
            foreach (AAMTGCard oCardWithoutNumber in m_OrderedCards)
                oCardWithoutNumber.number = iCardNumber++.ToString();
            return m_OrderedCards;
        }

        /// <summary>
        /// Returns the card with the given name
        /// </summary>
        /// <param name="p_sCardName">Card name</param>
        /// <returns>The wanted class</returns>
        public AAMTGCard GetCard(string p_sCardName)
        {
            foreach (AAMTGCard oCard in cards)
            {
                if (oCard.name == p_sCardName)
                    return oCard;
            }
            return null;
        }

    }

    class AAMTGCard
    {

        #region JASON INFO
        public string id { get; set; }                  //	3129aee7f26a4282ce131db7d417b1bc3338c4d4 A unique id for this card.It is made up by doing an SHA1 hash of setCode + cardName + cardImageName
        public string layout { get; set; }              //	"normal"	The card layout.Possible values: normal, split, flip, double-faced, token, plane, scheme, phenomenon, leveler, vanguard
        public string name { get; set; }                //"Research"	The card name.For split, double-faced and flip cards, just the name of one side of the card.Basically each 'sub-card' has its own record.
        public List<string> names { get; set; }         //["Research", "Development"] Only used for split, flip and double-faced cards. Will contain all the names on this card, front or back.
        public string manaCost { get; set; }            //    "{G}{U}"	The mana cost of this card.Consists of one or more mana symbols.
        public float cmc { get; set; }                  // 2	Converted mana cost.Always a number.NOTE: cmc may have a decimal point as cards from unhinged may contain "half mana" (such as 'Little Girl' with a cmc of 0.5). Cards without this field have an implied cmc of zero as per rule 202.3a
        public List<string> colors { get; set; }        //["Blue", "Green"] The card colors.Usually this is derived from the casting cost, but some cards are special (like the back of double-faced cards and Ghostfire).
        public List<string> colorIdentity { get; set; } // [ "U", "G" ] //This is created reading all card color information and costs. It is the same for double-sided cards(if they have different colors, the identity will have both colors). It also identifies all mana symbols in the card(cost and text). Mostly used on commander decks.
        public string type { get; set; }                //	"Legendary Creature — Angel"	The card type.This is the type you would see on the card if printed today.Note: The dash is a UTF8 'long dash' as per the MTG rules
        public List<string> supertypes { get; set; }    //["Legendary"] The supertypes of the card. These appear to the far left of the card type. Example values: Basic, Legendary, Snow, World, Ongoing
        public List<string> types { get; set; }         //   [ "Creature" ]  The types of the card.These appear to the left of the dash in a card type.Example values: Instant, Sorcery, Artifact, Creature, Enchantment, Land, Planeswalker
        public List<string> subtypes { get; set; }      //[ "Angel" ] The subtypes of the card.These appear to the right of the dash in a card type.Usually each word is its own subtype.Example values: Trap, Arcane, Equipment, Aura, Human, Rat, Squirrel, etc.
        public string rarity { get; set; }              //"Rare"	The rarity of the card.Examples: Common, Uncommon, Rare, Mythic Rare, Special, Basic Land
        public string text { get; set; }                //"{T}: You gain 1 life."	The text of the card.May contain mana symbols and other symbols.
        public string flavor { get; set; }              //"I'd like to buy a bowel."	The flavor text of the card.
        public string artist { get; set; }              //"Mark Poole"	The artist of the card.This may not match what is on the card as MTGJSON corrects many card misprints.
        public string number { get; set; }              //"148a"	The card number.This is printed at the bottom-center of the card in small text. This is a string, not an integer, because some cards have letters in their numbers.
        public string mcinumber { get; set; }           // ---> ANT Edit : sometines number is empty. mcinumber is instead found in data. so it is used instead.
        public string power { get; set; }               //"4"	The power of the card.This is only present for creatures.This is a string, not an integer, because some cards have powers like: "1+*"
        public string toughness { get; set; }           //"5"	The toughness of the card.This is only present for creatures.This is a string, not an integer, because some cards have toughness like: "1+*"
        public int loyalty { get; set; }                //4	The loyalty of the card.This is only present for planeswalkers.
        public int multiverseid { get; set; }           //2479	The multiverseid of the card on Wizard's Gatherer web page. Cards from sets that do not exist on Gatherer will NOT have a multiverseid.
                                                        //Sets not on Gatherer are: ATH, ITP, DKM, RQS, DPA and all sets with a 4 letter code that starts with a lowercase 'p'.
        public List<string> variations { get; set; }    //[1909, 1910] If a card has alternate art (for example, 4 different Forests, or the 2 Brothers Yamazaki) then each other variation's multiverseid will be listed here, NOT including the current card's multiverseid.NOTE: Only present for sets that exist on Gatherer.
        public string imageName { get; set; }           //"ajani goldmane"	This used to refer to the mtgimage.com file name for this card. mtgimage.com has been SHUT DOWN by Wizards of the Coast. This field will continue to be set correctly and is now only useful for UID purposes.
        public string watermark { get; set; }           //"Selesnya"	The watermark on the card.Note: Split cards don't currently have this field set, despite having a watermark on each side of the split card.
        public string border { get; set; }              //"black"	If the border for this specific card is DIFFERENT than the border specified in the top level set JSON, then it will be specified here. (Example: Unglued has silver borders, except for the lands which are black bordered)
        public bool timeshifted { get; set; }           //true	If this card was a timeshifted card in the set.
        public int hand { get; set; }                   //-3	Maximum hand size modifier. Only exists for Vanguard cards.
        public int life { get; set; }                   //-10	Starting life total modifier. Only exists for Vanguard cards.
        public bool reserved { get; set; }              //true	Set to true if this card is reserved by Wizards Official Reprint Policy
        public string releaseDate { get; set; }         //"2010-07-22" or "2010-07" or "2010"	The date this card was released.This is only set for promo cards. The date may not be accurate to an exact day and month, thus only a partial date may be set(YYYY-MM-DD or YYYY-MM or YYYY). Some promo cards do not have a known release date.
        public bool starter { get; set; }               //true	Set to true if this card was only released as part of a core box set. These are technically part of the core sets and are tournament legal despite not being available in boosters.
        #endregion

        #region Our own class values
        // Our own values, magiccards.info url construction
        public string sImageString { get { return GetImageString(); } }
        public Image oImage;
        public AAMTGSet oParentSet;
        public bool bIsBlue { get { return (this.IsColor("Blue")); } }
        public bool bIsBlack { get { return (this.IsColor("Black")); } }
        public bool bIsRed { get { return (this.IsColor("Red")); } }
        public bool bIsWhite { get { return (this.IsColor("White")); } }
        public bool bIsGreen { get { return (this.IsColor("Green")); } }
        private int m_iColorNbr = -1;
        public int iColorNbr { get { return (m_iColorNbr == -1 ? CountCardColors() : m_iColorNbr); } }
        public bool bIsInstant { get { return (this.IsType("Instant")); } }
        public bool bIsSorcery { get { return (this.IsType("Sorcery")); } }
        public bool bIsArtefact { get { return (this.IsType("Artifact")); } }
        public bool bIsCreature { get { return (this.IsType("Creature")); } }
        public bool bIsEnchantment { get { return (this.IsType("Enchantment")); } }
        public bool bIsLand { get { return (this.IsType("Land")); } }
        public bool bIsPlaneswalker { get { return (this.IsType("Planeswalker")); } }
        #endregion

        public AAMTGCard()
        {

        }


        /// <summary>
        /// Builds a string with cards information and returns it
        /// </summary>
        /// <returns>A string with various card info</returns>
        public string GetCardInfos()
        {
            return string.Format("Name:{0} MCICode:{1} number:{2} numberOfColors:{3}", this.name, this.oParentSet.magicCardsInfoCode, this.number, this.iColorNbr);
        }

        #region Various properties getters (Color, Type, ColorCount)

        /// <summary>
        /// Is this this card of the given Color?
        /// </summary>
        /// <param name="p_sColor">"Black" or "Red" for example</param>
        /// <returns>true or false</returns>
        private bool IsColor(string p_sColor)
        {
            if (this.colors != null && this.colors.Contains(p_sColor))
                return true;
            return false;
        }

        /// <summary>
        /// Is this this card of the given type?
        /// </summary>
        /// <param name="p_sType">"Artefact" or "Land" for example</param>
        /// <returns>true or false</returns>
        private bool IsType(string p_sType)
        {
            if (this.types != null && this.types.Contains(p_sType))
                return true;
            return false;
        }

        /// <summary>
        /// Returns how many color a card has
        /// </summary>
        /// <returns>An integer from 0 to 5 (so far)</returns>
        private int CountCardColors()
        {
            int iColors = 0;
            if (this.bIsBlack)
                iColors++;
            if (this.bIsBlue)
                iColors++;
            if (this.bIsGreen)
                iColors++;
            if (this.bIsWhite)
                iColors++;
            if (this.bIsRed)
                iColors++;
            m_iColorNbr = iColors;
            return iColors;
        }
        #endregion

        #region Image url generation and image download

        public string GetImageString()
        {
            if (oParentSet.magicCardsInfoCode == "5e")
                return string.Format("http://magiccards.info/scans/en/5e/{0}.jpg", this.mcinumber.Remove(0, 6));
            else if (oParentSet.magicCardsInfoCode == "4e")
                return string.Format("http://magiccards.info/scans/en/4e/{0}.jpg", this.mcinumber.Remove(0, 6));
            else
                return string.Format("http://magiccards.info/scans/en/{0}/{1}.jpg", oParentSet.magicCardsInfoCode == null ? oParentSet.code.ToLower() : oParentSet.magicCardsInfoCode, this.number == null ? this.mcinumber : this.number);
        }

        public int FetchImage()
        {
            if (this.oImage == null)
            {
                // Create web client.
                WebClient oClient = new WebClient();

                // Set user agent and also accept-encoding headers.
                oClient.Headers["User-Agent"] = "Googlebot/2.1 (+http://www.googlebot.com/bot.html)";
                oClient.Headers["Accept-Encoding"] = "gzip";

                // Download data.
                byte[] aFileBytes = oClient.DownloadData(this.sImageString);
                string sFileType = oClient.ResponseHeaders[HttpResponseHeader.ContentType];

                if (sFileType == "image/jpeg" || sFileType == "image/gif" || sFileType == "image/png")
                {
                    this.oImage = Image.FromStream(new MemoryStream(aFileBytes));
                }
            }
            return 1;
        }
        #endregion
    }
}
