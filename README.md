# UnifiedEmail

📬 여러 이메일 계정을 통합 관리할 수 있는 WPF MVVM 기반 이메일 클라이언트입니다. Gmail, Naver 등 다양한 메일 서비스를 지원하며, 메일 수신/발신/삭제/검색/채밀파일 기능까지 제공합니다.

---

## 기술 스택

* WPF (.NET 8)
* MVVM 아키텍쳐 (CommunityToolkit.Mvvm)
* MailKit / MimeKit (메일 송수신)
* SQLite (이메일 계정 저장)
* AES 대치키 암호화 (비밀로 보안 저장)

---

## 주요 기능

| 기능            | 설명                                         |
| ------------- | ------------------------------------------ |
| 계정 등록         | Gmail, Gsuite, Naver 등 다양한 메일 계정을 추가 등록 가능 |
| 메일 수신         | IMAP을 통한 메일 수신 및 포더별 분류 (받은메일함/보내메일함 등)    |
| 메일 발신         | SMTP를 통해 메일 작성 및 전송 가능                     |
| 메일 삭제         | 받은 메일 목록에서 개별 메일 삭제 기능 지원                  |
| 채밀파일 다운로드     | 수신된 채밀파일 확인 및 저장 가능                        |
| 메일 검색         | 제목, 발신자 기준 키워드 검색 기능 지원                    |
| 읽음 처리         | 더블클릭 시 읽음 처리 및 UI에서 시각적 변화(Bold → Normal)  |
| 자동 새로고침       | 60초 주기로 메일 목록 자동 갱신                        |
| MVVM 메시지 토스토스 | 계정 등록/삭제 후 ViewModel 간 메시지 기반 동기화 처리       |

---

## 폴더 구조

```
UnifiedEmail/
├── Models/              # EmailAccountModel, EmailMessageModel 등
├── Services/            # EmailService, DatabaseService, EncryptionService
├── ViewModels/          # EmailListViewModel, AccountViewModel 등
├── Views/               # EmailListView.xaml, ComposeEmailView.xaml 등
├── Messages/            # Messenger 메시지 객체 (예: RefreshEmailListMessage)
├── Screenshots/         # README 이미지 포더
└── UnifiedEmail.sln     # 솔링션 진입 파일
```

---

## 스크린샷

### 계정 등록 화면

![계정등록](Screenshots/AccountManage.png)

### 메일 작성

![작성](Screenshots/compose.png)

### 메일 본문 및 채밀

![상세](Screenshots/Detail.png)

### 전체 메일 목록

![목록](Screenshots/EmailList.png)

### 검색/필터 적용

![검색](Screenshots/SearchFilter.png)

---

## 주의사항

* 메일 계정 비밀로는 SQLite에 AES 방식으로 암호화되어 저장됩니다.
* Gmail 사용 시 IMAP 허용 및 앱 비밀번호 발급이 필요할 수 있습니다.
* Naver 사용 시 POP3/IMAP 사용 설정이 필요합니다.

---
