// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using EntityClasses.DomainEvents;
using EntityClasses.SupportClasses;
using GenericEventRunner.DomainParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityClasses
{
    public class Book : EntityEventsBase, ICreatedUpdated
    {

        private HashSet<Review> _reviews;

        public int BookId { get; private set; }
        public string Title { get; private set; }

        public DateTime WhenCreatedUtc { get; private set; }
        public DateTime LastUpdatedUtc { get; private set; }
        public void LogChange(bool added, EntityEntry entry)
        {
            var timeNow = DateTime.UtcNow;
            LastUpdatedUtc = timeNow;
            if (added)
            {
                WhenCreatedUtc = timeNow;
            }
            else
            {
                entry.Property(nameof(ICreatedUpdated.LastUpdatedUtc))
                    .IsModified = true;
            }
        }

        public IReadOnlyCollection<Review> Reviews => _reviews?.ToList();

        private Book(){}

        public static Book CreateBookWithEvent(string title)
        {
            var result = new Book
            {
                Title = title,
                _reviews = new HashSet<Review>()
            };
            result.AddEvent(new NewBookEvent(), EventToSend.DuringSave);
            return result;
        }

        public void ChangeTitle(string newTitle)
        {
            Title = newTitle;
        }

        public void AddReview(int numStars, string comment, string voterName)
        {
            if (_reviews == null)
                throw new InvalidOperationException("The Reviews collection must be loaded before calling this method");
            _reviews.Add(new Review(numStars, comment, voterName));
        }


        //This works with the GenericServices' IncludeThen Attribute to pre-load the Reviews collection
        public void RemoveReview(int reviewId)
        {
            if (_reviews == null)
                throw new InvalidOperationException("The Reviews collection must be loaded before calling this method");
            var localReview = _reviews.SingleOrDefault(x => x.ReviewId == reviewId);
            if (localReview == null)
                throw new InvalidOperationException("The review with that key was not found in the book's Reviews.");
            _reviews.Remove(localReview);
        }
    }
}