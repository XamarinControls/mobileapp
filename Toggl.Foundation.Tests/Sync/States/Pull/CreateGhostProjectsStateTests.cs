﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Pull
{
    public sealed class CreateGhostProjectsStateTests
    {
        private readonly IProjectsSource dataSource = Substitute.For<IProjectsSource>();
        private readonly CreateGhostProjectsState state;

        public CreateGhostProjectsStateTests()
        {
            state = new CreateGhostProjectsState(dataSource);
        }

        [Fact]
        public async Task ReturnsSuccessResultWhenNoTimeEntriesAreFetched()
        {
            var fetchObservables = fetch();

            var transition = await state.Start(fetchObservables);

            transition.Result.Should().Be(state.FinishedPersisting);
        }

        [Fact]
        public async Task ReturnsSuccessResultWhenTimeEntriesDoNotHaveProjectsOrTheProjectsAreInTheDatabase()
        {
            var fetchObservables = fetch(new MockTimeEntry { ProjectId = null }, new MockTimeEntry { ProjectId = 123 });
            dataSource.GetAll(Arg.Any<Func<IDatabaseProject, bool>>())
                .Returns(Observable.Return<IEnumerable<IThreadSafeProject>>(new[] { new MockProject() }));

            var transition = await state.Start(fetchObservables);

            transition.Result.Should().Be(state.FinishedPersisting);
        }

        [Fact]
        public async Task CreatesAGhostProjectIfThereIsNoProjectWithGivenIdInTheDatabase()
        {
            var timeEntry = new MockTimeEntry { ProjectId = 123, WorkspaceId = 456 };
            var fetchObservables = fetch(timeEntry);
            dataSource.GetAll(Arg.Any<Func<IDatabaseProject, bool>>())
                .Returns(Observable.Return<IEnumerable<IThreadSafeProject>>(new IThreadSafeProject[0]));

            await state.Start(fetchObservables);

            await dataSource.Received().Create(Arg.Is<IThreadSafeProject>(dto =>
                dto.Id == timeEntry.ProjectId.Value
                && dto.WorkspaceId == timeEntry.WorkspaceId
                && dto.SyncStatus == SyncStatus.RefetchingNeeded));
        }

        [Fact]
        public async Task CreatesOnlyOneGhostWhenMultipleTimeEntriesUseSameUnknownProject()
        {
            var projectId = 123;
            var timeEntryA = new MockTimeEntry { ProjectId = projectId, WorkspaceId = 456 };
            var timeEntryB = new MockTimeEntry { ProjectId = projectId, WorkspaceId = 456 };
            var fetchObservables = fetch(timeEntryA, timeEntryB);
            dataSource.GetAll(Arg.Any<Func<IDatabaseProject, bool>>())
                .Returns(Observable.Return<IEnumerable<IThreadSafeProject>>(new IThreadSafeProject[0]));

            await state.Start(fetchObservables);

            await dataSource.Received(1).Create(Arg.Is<IThreadSafeProject>(dto =>
                dto.Id == projectId && dto.SyncStatus == SyncStatus.RefetchingNeeded));
        }

        [Fact]
        public async Task DoesNotCreateAGhostWhenTheProjectIsInTheDatabase()
        {
            var timeEntry = new MockTimeEntry { ProjectId = 123 };
            var fetchObservables = fetch(timeEntry);
            dataSource.GetAll(Arg.Any<Func<IDatabaseProject, bool>>())
                .Returns(Observable.Return<IEnumerable<IThreadSafeProject>>(new[] { new MockProject() }));

            await state.Start(fetchObservables);

            await dataSource.DidNotReceive().Create(Arg.Any<IThreadSafeProject>());
        }

        [Fact]
        public async Task TheCreatedGhostProjectIsNotActive()
        {
            var timeEntry = new MockTimeEntry { ProjectId = 123, WorkspaceId = 456 };
            var fetchObservables = fetch(timeEntry);
            dataSource.GetAll(Arg.Any<Func<IDatabaseProject, bool>>())
                .Returns(Observable.Return<IEnumerable<IThreadSafeProject>>(new IThreadSafeProject[0]));

            await state.Start(fetchObservables);

            await dataSource.Received().Create(
                Arg.Is<IThreadSafeProject>(project => project.Active == false));
        }

        private IFetchObservables fetch(params ITimeEntry[] timeEntries)
        {
            var fetchObservables = Substitute.For<IFetchObservables>();
            fetchObservables.GetList<ITimeEntry>().Returns(Observable.Return(timeEntries.ToList()));
            return fetchObservables;
        }
    }
}
