---
name: Redacted Minimalist Registry
colors:
  surface: '#f7fafc'
  surface-dim: '#d7dadc'
  surface-bright: '#f7fafc'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f1f4f6'
  surface-container: '#ebeef0'
  surface-container-high: '#e5e9eb'
  surface-container-highest: '#e0e3e5'
  on-surface: '#181c1e'
  on-surface-variant: '#5b403e'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eef1f3'
  outline: '#8f6f6d'
  outline-variant: '#e4beba'
  surface-tint: '#b91c24'
  primary: '#b51822'
  on-primary: '#ffffff'
  primary-container: '#d93537'
  on-primary-container: '#fffbff'
  inverse-primary: '#ffb3ad'
  secondary: '#585e6c'
  on-secondary: '#ffffff'
  secondary-container: '#dde2f3'
  on-secondary-container: '#5e6473'
  tertiary: '#00666c'
  on-tertiary: '#ffffff'
  tertiary-container: '#008188'
  on-tertiary-container: '#f4ffff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#ffdad7'
  primary-fixed-dim: '#ffb3ad'
  on-primary-fixed: '#410004'
  on-primary-fixed-variant: '#930013'
  secondary-fixed: '#dde2f3'
  secondary-fixed-dim: '#c1c6d7'
  on-secondary-fixed: '#161c27'
  on-secondary-fixed-variant: '#414754'
  tertiary-fixed: '#8cf2fa'
  tertiary-fixed-dim: '#6fd6dd'
  on-tertiary-fixed: '#002022'
  on-tertiary-fixed-variant: '#004f53'
  background: '#f7fafc'
  on-background: '#181c1e'
  surface-variant: '#e0e3e5'
typography:
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 26px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  base: 4px
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 32px
  margin-mobile: 20px
  gutter-mobile: 12px
---

## Brand & Style

This design system is built for a high-utility check-in registry where speed and clarity are paramount. The personality is efficient, authoritative, and clinical, prioritizing function over decorative elements. 

The aesthetic follows a **Modern Minimalist** approach with a **High-Contrast** edge. It utilizes a stark white canvas punctuated by a singular, aggressive primary red to signal action and presence. The interface relies on structural integrity, generous whitespace, and precise alignment rather than shadows or gradients to create hierarchy. The goal is to evoke a sense of organized reliability for users who need to complete a task in seconds.

## Colors

The palette is intentionally restricted to maintain focus and reduce cognitive load. 

- **Primary (#E53E3E):** Reserved exclusively for critical actions, active states, and brand presence. It serves as the "focal point" color.
- **Surface (#FFFFFF):** The base for all screens to ensure maximum readability and a clean, sanitary feel.
- **Contrast (#1A202C):** Used for primary text and high-level icons to ensure accessibility compliance.
- **Muted (#718096):** Applied to secondary information and placeholder text.
- **Subtle (#EDF2F7):** Used for structural boundaries like dividers, disabled states, and input backgrounds.

## Typography

The typography uses **Inter**, a typeface designed for screen readability. The system employs a tight scale to ensure information density remains manageable on mobile devices. 

Headlines use bold weights and slight negative letter-spacing to appear "compact" and modern. Body text maintains a standard 1.5x line height for optimal legibility during data entry. Labels use a slightly heavier weight to distinguish them clearly from input data. All type is set in dark neutrals or the primary red (for error/critical states only).

## Layout & Spacing

This system utilizes a **Fluid Grid** model optimized for mobile viewport widths. A 4px baseline rhythm governs all vertical spacing to maintain a mathematical harmony between elements.

- **Margins:** 20px on the left and right edges to provide "breathing room" on small screens.
- **Vertical Rhythm:** Components are typically separated by 16px (md) or 24px (lg).
- **Touch Targets:** All interactive elements maintain a minimum height of 48px to ensure ease of use for varied physical capabilities.
- **Stacking:** Form fields and list items should span the full width of the content area (100% width minus margins).

## Elevation & Depth

To adhere to the minimalist-flat aesthetic, this design system **avoids shadows and blurs**. Depth is communicated through:

- **Flat Tonal Layering:** The primary background is White (#FFFFFF). Secondary sections or headers may use the Subtle Gray (#EDF2F7) to create a distinct container.
- **Stroke/Outline:** Elements like input fields and cards use a 1px solid border (#EDF2F7).
- **High Contrast:** Active states are indicated by the transition from a neutral border to the Primary Red (#E53E3E) border, or a solid Primary Red fill.

## Shapes

The design system uses a **Soft (Level 1)** roundedness. 

A corner radius of 4px (0.25rem) is applied to standard elements like input fields, buttons, and small containers. This subtle rounding prevents the UI from feeling overly harsh or "brutalist" while maintaining a professional, structured appearance. Large cards may use 8px (0.5rem) to differentiate them as primary layout containers.

## Components

### Buttons
- **Primary:** Solid Primary Red (#E53E3E) with White text. Bold weight, 4px radius.
- **Secondary:** White background with 1px border (#EDF2F7). Text is Contrast (#1A202C).
- **Ghost:** No background or border. Primary Red text. Used for "Cancel" or "Back" actions.

### Input Fields
- **Default:** White background, 1px border (#EDF2F7). Placeholder text in Muted Gray (#718096).
- **Active/Focus:** 1px border becomes Primary Red (#E53E3E).
- **Label:** Always positioned above the input in `label-md` style.

### Lists
- Registry entries are displayed in flat rows with a 1px bottom border (#EDF2F7).
- Icons within lists (e.g., "Check-in time") use Primary Red for emphasis.

### Chips/Badges
- Used for status (e.g., "Checked In"). Small caps, 2px radius. 
- Positive states use a very light tint of red or neutral gray to keep the focus on the primary action button.

### Cards
- Simple containers with a 1px border (#EDF2F7). No shadow. 
- Used to group related registration data, such as "Guest Information" or "Visit Details."