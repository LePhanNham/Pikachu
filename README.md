# 🎮 PIKACHU MATCH-3 GAME FLOW DESIGN

## 📋 TỔNG QUAN LUỒNG GAME

### 🎯 Mục tiêu
- Tạo một game match-3 với cơ chế nối các ô giống nhau bằng đường đi có tối đa 2 lần rẽ
- Hệ thống level progression với độ khó tăng dần
- Scoring system thông minh với bonus cho combo và quick match
- Hint system và auto-solve để hỗ trợ người chơi

---

## 🔄 LUỒNG GAME CHÍNH

### 1. **MAIN MENU** 🏠
```
┌─────────────────────────────────────┐
│           PIKACHU MATCH-3          │
│                                     │
│         [PLAY GAME]                 │
│         [SETTINGS]                  │
│         [QUIT]                      │
└─────────────────────────────────────┘
```

**Chức năng:**
- **Play Game**: Bắt đầu game mới
- **Settings**: Cài đặt âm thanh, đồ họa
- **Quit**: Thoát game

---

### 2. **GAME PLAY** 🎮

#### 2.1 Khởi tạo Level
```
┌─────────────────────────────────────┐
│ Level 1 | ⏰ 5:00 | 💰 0/1000      │
│                                     │
│ [PAUSE] [HINT] [SHUFFLE]           │
│                                     │
│         GAME BOARD                  │
│      (8x8 grid)                    │
└─────────────────────────────────────┘
```

**Thông tin hiển thị:**
- **Level**: Hiện tại đang chơi
- **Timer**: Thời gian còn lại (giảm theo level)
- **Score Progress**: Điểm hiện tại / Điểm cần đạt
- **Buttons**: Pause, Hint, Shuffle

#### 2.2 Gameplay Loop
```
1. Player click chọn ô đầu tiên → Highlight màu vàng
2. Player click chọn ô thứ hai → Highlight màu vàng
3. Kiểm tra có thể nối được không:
   ├─ ✅ CÓ: Xóa 2 ô + Tính điểm + Hiệu ứng
   └─ ❌ KHÔNG: Reset màu về trắng
4. Kiểm tra còn cặp nào không:
   ├─ ✅ CÓ: Tiếp tục chơi
   └─ ❌ KHÔNG: Shuffle board + Reset combo
```

#### 2.3 Scoring System
```
Base Score per Match: 100 điểm
Bonus System:
├─ Đường thẳng (0 rẽ): +0 bonus
├─ 1 lần rẽ: +50 bonus
├─ 2 lần rẽ: +100 bonus
├─ Quick Match (<3s): +50 bonus
├─ Combo: +25 điểm mỗi combo
└─ No Hint: +25 bonus
```

---

### 3. **PAUSE MENU** ⏸️
```
┌─────────────────────────────────────┐
│           GAME PAUSED              │
│                                     │
│         [RESUME]                    │
│         [RESTART LEVEL]             │
│         [MAIN MENU]                 │
└─────────────────────────────────────┘
```

**Chức năng:**
- **Resume**: Tiếp tục game
- **Restart Level**: Chơi lại level hiện tại
- **Main Menu**: Về menu chính

---

### 4. **VICTORY SCREEN** 🎉
```
┌─────────────────────────────────────┐
│           LEVEL COMPLETE!           │
│                                     │
│         Score: 1250                 │
│         Time Bonus: +200            │
│         Total: 1450                 │
│                                     │
│         [NEXT LEVEL]                │
│         [MAIN MENU]                 │
└─────────────────────────────────────┘
```

**Thông tin hiển thị:**
- Điểm đạt được
- Bonus thời gian
- Tổng điểm
- Nút chuyển level tiếp theo

---

### 5. **GAME OVER SCREEN** ⏰
```
┌─────────────────────────────────────┐
│           TIME'S UP!                │
│                                     │
│         Final Score: 850            │
│         Level Reached: 3            │
│                                     │
│         [RESTART LEVEL]             │
│         [MAIN MENU]                 │
└─────────────────────────────────────┘
```

**Thông tin hiển thị:**
- Điểm cuối cùng
- Level đã đạt được
- Nút chơi lại hoặc về menu

---

## 🎯 HỆ THỐNG LEVEL

### Level Progression
```
Level 1: 5:00 - Target: 1000 điểm
Level 2: 4:50 - Target: 1200 điểm
Level 3: 4:40 - Target: 1400 điểm
...
Level 10: 3:00 - Target: 2800 điểm
```

### Difficulty Scaling
- **Thời gian**: Giảm 10 giây mỗi level
- **Điểm cần đạt**: Tăng 200 điểm mỗi level
- **Board size**: Có thể tăng từ 8x8 lên 10x10 ở level cao

---

## 🧠 HINT SYSTEM

### Cách hoạt động
1. **Auto-detect**: Tự động tìm tất cả cặp có thể nối
2. **Best Pair**: Gợi ý cặp có đường đi ngắn nhất
3. **Visual Hint**: Highlight 2 ô và hiển thị đường đi
4. **Cost**: Mỗi level chỉ được dùng 1 lần

### Thuật toán Hint
```
1. Tìm tất cả cặp có thể nối được
2. Tính độ khó của mỗi cặp (số lần rẽ)
3. Chọn cặp có độ khó thấp nhất
4. Highlight và hiển thị đường đi
```

---

## 🎨 VISUAL EFFECTS

### Animation System
- **Tile Selection**: Flash màu vàng
- **Tile Removal**: Fade out + particle effect
- **Board Shuffle**: Fade out → Shuffle → Fade in
- **Path Drawing**: Line renderer với màu xanh lá

### Color Coding
- **Normal**: Trắng
- **Selected**: Vàng
- **Hint**: Cyan
- **Path**: Xanh lá
- **Empty**: Trong suốt

---

## 🔊 AUDIO SYSTEM

### Sound Effects
- **Click**: Khi chọn ô
- **Match**: Khi nối được 2 ô
- **No Move**: Khi không thể nối
- **Victory**: Khi hoàn thành level
- **Game Over**: Khi hết thời gian

### Background Music
- **Main Menu**: Nhạc nền nhẹ nhàng
- **Gameplay**: Nhạc nền sôi động
- **Victory**: Nhạc chiến thắng
- **Defeat**: Nhạc buồn

---

## 🎮 GAMEPLAY MECHANICS

### Core Rules
1. **Matching**: Nối 2 ô giống nhau
2. **Path Finding**: Đường đi tối đa 2 lần rẽ
3. **Scoring**: Điểm dựa trên độ khó + bonus
4. **Time Limit**: Thời gian giới hạn mỗi level
5. **Level Progression**: Cần đạt điểm mục tiêu

### Advanced Features
- **Combo System**: Liên tiếp match không ngắt
- **Quick Match Bonus**: Thưởng cho match nhanh
- **Hint System**: Gợi ý khi gặp khó khăn
- **Auto Shuffle**: Tự động xáo khi hết nước đi
- **Progress Tracking**: Theo dõi tiến độ level

---

## 🔧 TECHNICAL IMPLEMENTATION

### Architecture
```
GameManager (State Machine)
├─ GameState Management
├─ Level System
├─ Scoring System
└─ Event System

BoardManager (Game Logic)
├─ Board Generation
├─ Tile Management
├─ Match Detection
└─ Effect System

PathFinder (Algorithm)
├─ BFS Path Finding
├─ Turn Counting
├─ Path Reconstruction
└─ Optimization

UIManager (Interface)
├─ Canvas Management
├─ Event Handling
├─ UI Updates
└─ Navigation
```

### Key Components
- **Singleton Pattern**: Quản lý global state
- **Event System**: Communication giữa components
- **Coroutine System**: Animation và timing
- **Object Pooling**: Performance optimization
- **State Machine**: Game flow control

---

## 🚀 FUTURE ENHANCEMENTS

### Planned Features
1. **Power-ups**: Bom, Lightning, Color Bomb
2. **Daily Challenges**: Mission system
3. **Leaderboard**: Online ranking
4. **Achievements**: Unlockable rewards
5. **Custom Themes**: Visual customization
6. **Multiplayer**: PvP mode

### Performance Optimizations
1. **GPU Instancing**: Batch rendering
2. **Object Pooling**: Memory management
3. **LOD System**: Level of detail
4. **Async Loading**: Background processing
5. **Mobile Optimization**: Touch controls

---

## 📱 PLATFORM SUPPORT

### Target Platforms
- **PC**: Windows, macOS, Linux
- **Mobile**: iOS, Android
- **Web**: HTML5, WebGL

### Control Schemes
- **PC**: Mouse click
- **Mobile**: Touch tap
- **Web**: Mouse/Touch hybrid

---

*Document created for Pikachu Match-3 Game Development*
*Version: 1.0 | Date: 2024*
