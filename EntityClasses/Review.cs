// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace EntityClasses
{
    public class Review
    {
        private Review() { }

        internal Review(int numStars, string comment, string voterName, int bookId = default)
        {
            NumStars = numStars;
            Comment = comment;
            VoterName = voterName;
            BookId = bookId;
        }

        public int ReviewId { get; private set; }

        public string VoterName { get; private set; }

        public int NumStars { get; private set; }
        public string Comment { get; private set; }

        //-----------------------------------------
        //Relationships

        public int BookId { get; private set; }
    }
}