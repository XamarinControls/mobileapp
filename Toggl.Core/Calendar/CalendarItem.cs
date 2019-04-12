﻿using System;
using Toggl.Core.Models.Interfaces;
using Toggl.Storage;
using ColorHelper = Toggl.Core.Helper.Color;

namespace Toggl.Core.Calendar
{
    public struct CalendarItem
    {
        public string Id { get; }

        public CalendarItemSource Source { get; }

        public TimeSpan? Duration { get; }

        public DateTimeOffset StartTime { get; }

        public DateTimeOffset? EndTime => StartTime + Duration;

        public string Description { get; }

        public string Color { get; }

        public long? TimeEntryId { get; }

        public string CalendarId { get; }

        public CalendarIconKind IconKind { get; }

        public CalendarItem(
            string id,
            CalendarItemSource source,
            DateTimeOffset startTime,
            TimeSpan? duration,
            string description,
            CalendarIconKind iconKind,
            string color = ColorHelper.NoProject,
            long? timeEntryId = null,
            string calendarId = "")
        {
            Id = id;
            Source = source;
            StartTime = startTime;
            Duration = duration;
            Description = string.IsNullOrEmpty(description) ? Resources.NoDescription : description;
            Color = ColorHelper.IsValidHexColor(color) ? color : ColorHelper.NoProject;
            TimeEntryId = timeEntryId;
            CalendarId = calendarId;
            IconKind = iconKind;
        }

        private CalendarItem(IThreadSafeTimeEntry timeEntry)
            : this(
                timeEntry.Id.ToString(),
                CalendarItemSource.TimeEntry,
                timeEntry.Start,
                timeEntry.Duration.HasValue ? TimeSpan.FromSeconds(timeEntry.Duration.Value) : null as TimeSpan?,
                timeEntry.Description,
                CalendarIconKind.None,
                timeEntry.Project?.Color ?? ColorHelper.NoProject,
                timeEntry.Id)
        {
            switch (timeEntry.SyncStatus)
            {
                case SyncStatus.SyncNeeded:
                    IconKind = CalendarIconKind.Unsynced;
                    break;
                case SyncStatus.SyncFailed:
                    IconKind = CalendarIconKind.Unsyncable;
                    break;
                default:
                    break;
            }
        }

        public static CalendarItem From(IThreadSafeTimeEntry timeEntry)
            => new CalendarItem(timeEntry);

        public CalendarItem WithStartTime(DateTimeOffset startTime)
            => new CalendarItem(
                this.Id,
                this.Source,
                startTime,
                this.Duration,
                this.Description,
                this.IconKind,
                this.Color,
                this.TimeEntryId,
                this.CalendarId);

        public CalendarItem WithDuration(TimeSpan? duration)
            => new CalendarItem(
                this.Id,
                this.Source,
                this.StartTime,
                duration,
                this.Description,
                this.IconKind,
                this.Color,
                this.TimeEntryId,
                this.CalendarId);
    }
}
