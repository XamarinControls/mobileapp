﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Core.Autocomplete.Suggestions;
using Toggl.Core.DataSources;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Core.Interactors.AutocompleteSuggestions
{
    internal sealed class GetTimeEntriesAutocompleteSuggestions : IInteractor<IObservable<IEnumerable<AutocompleteSuggestion>>>
    {
        private readonly ITimeEntriesSource dataSource;

        private readonly IList<string> wordsToQuery;

        public GetTimeEntriesAutocompleteSuggestions(ITimeEntriesSource dataSource, IList<string> wordsToQuery)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(wordsToQuery, nameof(wordsToQuery));

            this.dataSource = dataSource;
            this.wordsToQuery = wordsToQuery;
        }

        public IObservable<IEnumerable<AutocompleteSuggestion>> Execute()
            => wordsToQuery
                .Aggregate(dataSource.GetAll(), (obs, word) => obs.Select(filterByWord(word)))
                .Select(TimeEntrySuggestion.FromTimeEntries);

        private Func<IEnumerable<IThreadSafeTimeEntry>, IEnumerable<IThreadSafeTimeEntry>> filterByWord(string word)
            => timeEntries =>
                timeEntries.Where(
                    te => te.Description.ContainsIgnoringCase(word)
                        || (te.Project != null && te.Project.Name.ContainsIgnoringCase(word) && te.Project.Active)
                        || (te.Project?.Client != null && te.Project.Client.Name.ContainsIgnoringCase(word))
                        || (te.Task != null && te.Task.Name.ContainsIgnoringCase(word)));
    }
}
