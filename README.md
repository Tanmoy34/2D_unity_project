# 2D Platformer Game - Unity Project

## Project Overview

**2D Platformer** is a production-ready 2D side-scrolling platformer game built with Unity, demonstrating professional game development practices including modern input handling, physics-based movement, and optimized ground detection. The project serves as a robust foundation for building larger platformer games with extensible architecture and clean code patterns.

**Version:** 1.0.0  
**Unity Version:** 2021.3 LTS or newer (2022+ recommended)  
**License:** MIT  

---

## Key Features

### Core Gameplay Mechanics
- **Responsive Horizontal Movement** - Smooth acceleration and deceleration with configurable physics
- **Dynamic Jump System** - Variable jump height based on button hold duration with realistic gravity
- **Advanced Ground Detection** - Raycast-based grounding system for precise platform edge detection
- **Sprite Flipping** - Automatic character orientation based on movement direction

### Technical Implementation
- **New Input System Integration** - Modern callback-based input handling supporting both keyboard and gamepad
- **Physics-Based Movement** - Rigidbody2D integration for natural movement feel and collision handling
- **Multiple Ground Raycasts** - Three simultaneous raycasts for reliable ground detection without overlap checks
- **Smart Camera System** - Smooth following with configurable offset and lag
- **Procedural Level Generation** - Automatic platform spawning system for rapid prototyping

### Performance & Architecture
- Clean separation of concerns with modular script design
- Optimized raycasting for ground detection (3 rays per frame)
- Configurable physics parameters for quick iteration
- Debug visualization system for ground detection debugging

---

## Technical Specifications

### System Requirements

| Requirement | Minimum | Recommended |
|---|---|---|
| Unity Version | 2021.3 LTS | 2022.3+ |
| Platform | Windows, macOS, Linux | Windows, macOS, Linux |
| RAM | 4GB | 8GB+ |
| Input System | Input System Package v1.2+ | Latest |

### Project Dependencies

```
Unity Engine (2021.3+)
├── Input System Package (v1.2.0+)
├── Physics2D (built-in)
└── TextMeshPro (built-in)
```

---

## Architecture Overview

### Script Hierarchy

```
PlayerController.cs
├── Movement System
│   ├── Horizontal Motion
│   ├── Acceleration/Deceleration
│   └── Sprite Flipping
├── Jump System
│   └── Grounded State Management
└── Ground Detection
    └── Raycast System (3 raycasts)

CameraFollow.cs
├── Position Tracking
├── Smooth Interpolation
└── Offset Configuration

PlatformGenerator.cs
├── Prefab Instantiation
├── Random Positioning
└── Scale Configuration

PlayerInputActions.inputactions
├── Keyboard Bindings
│   ├── WASD Movement
│   └── Space Jump
└── Gamepad Bindings
    ├── Left Stick Movement
    └── South Button Jump
```

### Component Relationships

```
Scene Hierarchy:
├── Main Camera
│   └── CameraFollow (tracks Player)
├── Player
│   ├── Rigidbody2D (physics)
│   ├── BoxCollider2D (collision)
│   └── PlayerController (logic)
├── PlatformManager
│   └── PlatformGenerator (spawns platforms)
└── Platforms (generated at runtime)
    ├── Platform (prefab instance)
    ├── BoxCollider2D (collision)
    └── Layer: Ground
```

---

## Ground Detection System

### Raycast Implementation

The ground detection system uses **three simultaneous raycasts** cast downward from the player's collider bottom:

```
╔════════════════╗
║     Player     ║
╠════════════════╣
↓ Left Ray  ↓ Mid Ray  ↓ Right Ray
```

**Advantages over Overlap Checks:**
- More precise edge detection
- Better performance consistency
- No false positives on collider edges
- Debug visualization for troubleshooting
- Configurable ray count for different collision shapes

### Ground Detection Process

```csharp
1. Get player collider bounds
2. Calculate 3 positions along bottom edge
3. Cast Physics2D.Raycast() downward for each position
4. Check if ANY ray hits Ground layer
5. Set isGrounded = true if collision detected
6. Draw debug rays (green = grounded, red = falling)
```

### Configuration Parameters

| Parameter | Default | Purpose |
|---|---|---|
| Ground Check Distance | 0.1 | How far to raycast below player |
| Ground Ray Count | 3 | Number of raycasts per frame |
| Ground Layer | "Ground" | Which layers to detect as ground |

---

## Input System Integration

### Action Bindings

**Move Action (Vector2)**
- **Keyboard:** WASD or Arrow Keys
- **Gamepad:** Left Stick (Thumbstick)
- **Type:** Value continuous input

**Jump Action (Button)**
- **Keyboard:** Spacebar
- **Gamepad:** South Button (A on Xbox, Cross on PlayStation)
- **Type:** Button with Tap interaction

### Input Flow Architecture

```
PlayerInputActions.inputactions
         ↓
PlayerController.Awake()
         ↓
inputActions.Player.Enable()
         ↓
Subscribe to callbacks:
├── Move.performed → OnMovePerformed()
├── Move.canceled → OnMoveCanceled()
└── Jump.performed → OnJumpPerformed()
         ↓
FixedUpdate() reads currentVelocity & jumpPressed
         ↓
Physics applied to Rigidbody2D
```

---

## Physics Configuration

### Movement Physics

| Parameter | Default | Recommended Range | Effect |
|---|---|---|---|
| Move Speed | 5.0 | 3-8 | Maximum horizontal velocity |
| Acceleration | 10.0 | 5-20 | How quickly player reaches max speed |
| Deceleration | 8.0 | 5-15 | How quickly player stops |

### Jump Physics

| Parameter | Default | Recommended Range | Effect |
|---|---|---|---|
| Jump Force | 5.0 | 3-8 | Initial jump impulse (m/s) |
| Normal Gravity Scale | 1.5 | 1-2 | Gravity while ascending |
| Fall Gravity Scale | 2.5 | 2-4 | Gravity while descending (faster fall) |

### Movement Feel Tuning

```
Snappy Feel:      Sluggish Feel:
Acceleration: 15  Acceleration: 5
Deceleration: 12  Deceleration: 3
Move Speed: 7     Move Speed: 3
Jump Force: 6     Jump Force: 4
```

---

## Installation & Setup

### Quick Start (5 minutes)

1. **Create Unity Project**
   ```bash
   Unity Hub → Create → 2D Project (2022+)
   ```

2. **Install Input System**
   - Window → Package Manager
   - Search "Input System" → Install
   - Restart Unity when prompted

3. **Import Scripts**
   - Copy all `.cs` files to `Assets/Scripts/`
   - Copy `PlayerInputActions.inputactions` to `Assets/Input/`

4. **Create Scene**
   - Create new scene: `MainGame`
   - Save to `Assets/Scenes/`

5. **Setup Objects**
   - Create Player (Square sprite, add Rigidbody2D, BoxCollider2D)
   - Create Platform prefab (Square sprite, BoxCollider2D)
   - Create PlatformManager (empty GameObject, add PlatformGenerator)
   - Setup Main Camera with CameraFollow script

6. **Configure & Play**
   - Assign components in inspectors
   - Set Ground layer on platforms
   - Press Play

See `SETUP_GUIDE.md` for detailed step-by-step instructions.

---

## API Reference

### PlayerController

**Public Properties**
```csharp
public bool IsGrounded { get; }  // Returns true if player is grounded
```

**Serializable Fields**
```csharp
[Header("Movement")]
float moveSpeed = 5f;              // Max horizontal velocity
float acceleration = 10f;          // Speed to reach max velocity
float deceleration = 8f;           // Speed to stop

[Header("Jump")]
float jumpForce = 5f;              // Jump impulse strength
float fallGravityScale = 2.5f;    // Gravity multiplier when falling
float normalGravityScale = 1.5f;  // Gravity multiplier when rising

[Header("Ground Detection - Raycast")]
float groundCheckDistance = 0.1f;  // Raycast distance downward
int groundRayCount = 3;            // Number of simultaneous raycasts
LayerMask groundLayer;             // Physics layers to detect as ground
```

**Input Callbacks**
```csharp
// Called by Input System
OnMovePerformed(InputAction.CallbackContext)  // Move input received
OnMoveCanceled(InputAction.CallbackContext)   // Move input released
OnJumpPerformed(InputAction.CallbackContext)  // Jump pressed
```

### CameraFollow

**Serializable Fields**
```csharp
Transform target;           // Player to follow
float smoothSpeed = 5f;     // Interpolation speed (higher = snappier)
Vector3 offset;             // Offset from target position
bool useSmoothing = true;   // Enable smooth following
```

### PlatformGenerator

**Serializable Fields**
```csharp
GameObject platformPrefab;      // Platform to instantiate
int platformCount = 5;          // Number of platforms to generate
float platformSpacing = 5f;     // Distance between platforms
float platformWidth = 3f;       // Platform scale width
float platformHeight = 0.5f;    // Platform scale height
Vector2 yRandomRange;           // Min/max Y position variance
```

---

## Extensibility & Customization

### Adding New Features

#### Double Jump
```csharp
// In PlayerController.cs, modify jump system:
private int jumpCount = 0;
private int maxJumps = 2;

private void HandleJump() {
    if (jumpPressed && jumpCount < maxJumps) {
        jumpPressed = false;
        jumpCount++;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}

// Reset jump count when grounded:
private void CheckGroundedWithRaycast() {
    // ... existing raycast code ...
    if (isGrounded) jumpCount = 0;
}
```

#### Wall Slide Detection
```csharp
private bool isOnWall;

private void CheckWallCollision() {
    Vector2 rayDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    RaycastHit2D wallHit = Physics2D.Raycast(transform.position, rayDirection, 0.3f);
    isOnWall = wallHit.collider != null && !isGrounded;
}
```

#### Dash Ability
```csharp
[SerializeField] private float dashSpeed = 15f;
[SerializeField] private float dashDuration = 0.3f;
private float dashTimer;

private void HandleDash() {
    if (dashTimer > 0) {
        currentVelocityX = moveInput.x * dashSpeed;
        dashTimer -= Time.fixedDeltaTime;
    }
}
```

### Visual Customization

- Replace square sprites with character artwork
- Add particle effects on jump/land
- Implement sprite animation controller
- Create visual feedback for ground detection
- Add UI elements (score, lives, etc.)

### Audio Implementation

```csharp
[SerializeField] private AudioClip jumpSFX;
[SerializeField] private AudioClip landSFX;

private AudioSource audioSource;

void Start() {
    audioSource = GetComponent<AudioSource>();
}

void Jump() {
    audioSource.PlayOneShot(jumpSFX);
}
```

---

## Performance Considerations

### Optimization Strategies

| Area | Current | Improvement | Impact |
|---|---|---|---|
| Ground Detection | 3 raycasts/frame | Increase to 5 for wider colliders | Minimal CPU cost |
| Camera Following | Lerp every frame | Consider FixedUpdate sync | Smoother motion |
| Platform Generation | Runtime spawn | Preload/cache prefabs | Reduce instantiation cost |
| Physics Update | Default 50Hz | Match game frame rate | Consistency |

### Profiling Recommendations

```
Profile Areas:
1. Ground detection raycast count
2. Camera smooth speed impact
3. Physics update frequency
4. Rigidbody sleep states
5. Collider complexity
```

---

## Known Issues & Limitations

### Current Limitations

1. **Single-Layer Ground Detection**
   - Currently detects only one "Ground" layer
   - Workaround: Use layer masks or add slope detection

2. **No Slope Handling**
   - Raycasts don't account for sloped platforms
   - Workaround: Use horizontal platforms or add slope angle checking

3. **Fixed Jump Height**
   - Jump force is constant regardless of held duration
   - Workaround: Implement button hold duration tracking

### Future Enhancements

- [ ] Slope support with raycast angle detection
- [ ] Coyote time (late jump after leaving platform)
- [ ] Jump buffering for responsive input
- [ ] Moving platforms support
- [ ] One-way platform collision
- [ ] Ledge grab mechanics
- [ ] Dash/air movement
- [ ] Enemy AI system
- [ ] Collectible/power-up system
- [ ] Level progression system

---

## Development Workflow

### Version Control

```bash
# Recommended .gitignore entries
Library/
Logs/
Temp/
obj/
*.csproj
*.sln
.DS_Store
```

### Build Process

```bash
# Standalone build from command line:
Unity.exe -projectPath . -executeMethod BuildManager.BuildPC -quit -batchmode
```

### Testing Checklist

- [ ] Movement acceleration/deceleration feels natural
- [ ] Jump height is consistent
- [ ] Ground detection works on all platform edges
- [ ] Camera doesn't clip through terrain
- [ ] Gamepad input responsive
- [ ] No physics glitches at high speeds
- [ ] Platform generation creates valid layouts

---

## Code Quality Standards

### Naming Conventions

```csharp
// Fields
private float moveSpeed;           // Camel case, private
[SerializeField] float duration;   // Camel case, SerializeField

// Methods
private void HandleMovement()      // PascalCase, descriptive

// Classes
public class PlayerController      // PascalCase

// Constants
private const float GRAVITY = 9.8f; // UPPER_SNAKE_CASE
```

### Documentation Standards

```csharp
/// <summary>
/// Checks if player is touching ground using raycasts.
/// </summary>
/// <remarks>
/// Uses 3 raycasts for better edge detection.
/// Raycasts are visualized as debug rays (green = grounded).
/// </remarks>
private void CheckGroundedWithRaycast()
{
    // Implementation
}
```

---

## Troubleshooting

### Common Issues

**Q: Player falls through platforms**
- Check platform has BoxCollider2D
- Verify platform is on "Ground" layer
- Increase groundCheckDistance to 0.15
- Ensure Rigidbody2D Body Type is Dynamic

**Q: Jump feels unresponsive**
- Verify Jump action is firing in Input System
- Check isGrounded variable in Play mode
- Increase Jump Force value
- Verify FixedUpdate is running (Physics2D)

**Q: Camera stutters**
- Reduce smoothSpeed value (lower = smoother)
- Check frame rate consistency
- Verify LateUpdate is being called
- Profile with Unity Profiler

**Q: Input not registering**
- Open PlayerInputActions.inputactions in Inspector
- Verify control schemes are configured
- Check that Player action map is enabled
- Test with different input devices

---

## Contributing

### Pull Request Guidelines

1. Create feature branch: `git checkout -b feature/new-feature`
2. Follow code standards outlined above
3. Add documentation for new features
4. Test thoroughly before submission
5. Submit PR with detailed description

### Reporting Bugs

Include:
- Unity version used
- Steps to reproduce
- Expected vs actual behavior
- Screenshot/video if applicable
- System specifications

---

## License

This project is licensed under the MIT License. See LICENSE file for details.

---

## Credits & Acknowledgments

**Development:** Game Development Team  
**Input System:** Unity Technologies  
**Physics Engine:** NVIDIA PhysX  

---

## Resources & References

### Official Documentation
- [Unity 2D Physics](https://docs.unity3d.com/Manual/Physics2DReference.html)
- [Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/)
- [Rigidbody2D Reference](https://docs.unity3d.com/ScriptReference/Rigidbody2D.html)

### Learning Resources
- Brackeys - 2D Platformer Tutorial Series
- Sebastian Lague - Platformer Movement Analysis
- Game Maker's Toolkit - Platformer Design Essentials

### Tools & Utilities
- Unity Profiler (built-in performance analysis)
- Frame Debugger (graphics analysis)
- Physics Debugger (collision visualization)

---

## Support & Contact

For questions, issues, or suggestions:
- **GitHub Issues:** [Submit issue]
- **Email:** support@gamedev.local
- **Discord:** [Join community server]

---

**Last Updated:** February 2026  
**Maintained By:** Development Team  
**Status:** Active Development
