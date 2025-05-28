using System;
using MailKit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UnifiedEmail.Models
{
    public partial class EmailMessageModel : ObservableObject
    {
        [ObservableProperty]
        private string subject;

        [ObservableProperty]
        private string from;

        [ObservableProperty]
        private DateTime date;

        [ObservableProperty]
        private UniqueId uniqueId;

        [ObservableProperty]
        private MessageFlags flags;

        public bool IsRead => Flags.HasFlag(MessageFlags.Seen);

        // Flags 값이 바뀔 때 UI에 IsRead도 바뀌었다고 알리기
        partial void OnFlagsChanged(MessageFlags oldValue, MessageFlags newValue)
        {
            OnPropertyChanged(nameof(IsRead));
        }
    }
}
