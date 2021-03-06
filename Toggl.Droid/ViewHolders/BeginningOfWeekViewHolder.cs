﻿using System;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Toggl.Core.UI.ViewModels;

namespace Toggl.Droid.ViewHolders
{
    public sealed class BeginningOfWeekViewHolder : BaseRecyclerViewHolder<SelectableBeginningOfWeekViewModel>
    {
        private TextView beginningOfWeekTextView;
        private RadioButton selectedButton;

        public static BeginningOfWeekViewHolder Create(View itemView)
            => new BeginningOfWeekViewHolder(itemView);

        public BeginningOfWeekViewHolder(View itemView)
            : base(itemView)
        {
        }

        public BeginningOfWeekViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            beginningOfWeekTextView = ItemView.FindViewById<TextView>(Resource.Id.BeginningOfWeekText);
            selectedButton = ItemView.FindViewById<RadioButton>(Resource.Id.BeginningOfWeekRadioButton); 
        }

        protected override void UpdateView()
        {
            // How to localize this?
            beginningOfWeekTextView.Text = Item.BeginningOfWeek.ToString();
            selectedButton.Checked = Item.Selected;
        }
    }
}
