﻿using System.Collections.Generic;
using Foundation;
using Toggl.Core.UI.Calendar;

namespace Toggl.Daneel.Views.Calendar
{
    public interface ICalendarCollectionViewLayoutDataSource
    {
        IEnumerable<NSIndexPath> IndexPathsOfCalendarItemsBetweenHours(int minHour, int maxHour);

        CalendarItemLayoutAttributes LayoutAttributesForItemAtIndexPath(NSIndexPath indexPath);

        NSIndexPath IndexPathForEditingItem();

        NSIndexPath IndexPathForRunningTimeEntry();
    }
}
