using CommunityToolkit.Mvvm.ComponentModel;
using MailKit;

namespace UnifiedEmail.Models
{
    // 이메일 메시지 정보 모델
    public partial class EmailMessageModel : ObservableObject
    {
        [ObservableProperty]
        private string subject; // 메일 제목

        [ObservableProperty]
        private string from; // 발신자

        [ObservableProperty]
        private DateTime date; // 발신 일시

        [ObservableProperty]
        private UniqueId uniqueId; // IMAP 고유 ID

        [ObservableProperty]
        private MessageFlags flags; // 메일 플래그(읽음 등)

        public bool IsRead => Flags.HasFlag(MessageFlags.Seen); // 읽음 여부

        // Flags 값 변경 시 IsRead 변경 알림
        partial void OnFlagsChanged(MessageFlags oldValue, MessageFlags newValue)
        {
            OnPropertyChanged(nameof(IsRead));
        }
    }
}
