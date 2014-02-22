using System.Collections.Generic;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    class Character : IResponseSchema
    {
        public string Aliases { get; set; }
        public string Api_Detail_Url { get; set; }
        public string Birth { get; set; }
        public IEnumerable<Character> Character_Enemies { get; set; }
        public IEnumerable<Character> Character_Friends { get; set; }
        public int Count_Of_Issue_Appearances { get; set; }
        public IEnumerable<Person> Creators { get; set; }
        public string Deck { get; set; }
        public string Description { get; set; }
        public Issue First_Appeared_In_Issue { get; set; }
        public int Gender { get; set; }
        public int Id { get; set; }
        public Images Image { get; set; }
        public string Name { get; set; }
        public Publisher Publisher { get; set; }
    }
}
