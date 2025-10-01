# Alchemy Book Entry Template System

## Overview
The Alchemy Book Entry Template system provides a unified UXML-based approach for displaying alchemy book entries, offering consistent styling and flexible content layout across all entry types.

## Files Created
- **UXML Template**: `/Assets/UI/UXML/AlchemyBookEntry.uxml`
- **Style Sheet**: `/Assets/UI/Styles/AlchemyBookEntry.uss`
- **Setup Tool**: `/Assets/Scripts/Editor/AlchemyBookTemplateSetup.cs`

## Quick Setup

### Option 1: Automatic Setup (Recommended)
1. Open `Tools > Alchemy Book Template Setup` in Unity
2. Click **"Auto-Setup AlchemyBook Template"**
3. The tool will automatically configure all assets

### Option 2: Manual Setup
1. Select your **AlchemyBookUI** GameObject in the scene
2. In the **AlchemyBook** component:
   - Assign `AlchemyBookEntry.uxml` to the **Entry Template** field
3. In the **UIDocument** component:
   - Add `AlchemyBookEntry.uss` to the **Style Sheets** list

## Template Features

### Template Elements
- **Header**: Title, subtitle, and bookmark button
- **Content**: Image/icon area and text content
- **Properties**: Configurable sections for different content types
- **Footer**: Page number and discovery status

### Entry Type Support
- **Bestiary Entries**: Red border, behavior and habitat info, dropped ingredients
- **Recipe Entries**: Green border, required ingredients and discovered infusions
- **Ingredient Entries**: Blue border, available refinements and drop sources
- **Help Entries**: Purple border, steps and tips

### Smart Content Display
- Sections only appear when they contain data
- Automatic type-specific styling
- Progressive discovery system support
- Bookmark functionality with visual feedback

## CSS Classes for Customization

### Main Container
```css
.book-entry          /* Base entry styling */
.bestiary-entry      /* Red border for creatures */
.recipe-entry        /* Green border for recipes */
.ingredient-entry    /* Blue border for ingredients */
.help-entry         /* Purple border for help */
```

### Layout Modes
```css
.book-entry.compact   /* Smaller entry size */
.book-entry.expanded  /* Larger entry size */
```

### Element Classes
```css
.entry-title         /* Main title text */
.entry-subtitle      /* Secondary info */
.entry-description   /* Main description */
.bookmark-button     /* Star bookmark */
.bookmark-button.bookmarked /* Active bookmark */
```

## Fallback System
The template system includes automatic fallback:
- If no template is assigned, uses original `CreateEntryVisual()` methods
- Maintains compatibility with existing entries
- Graceful degradation if template elements are missing

## Advanced Customization

### Adding New Sections
1. Add new elements to the UXML template
2. Extend the appropriate `PopulateXXXContent()` method
3. Add corresponding CSS styling

### Creating Custom Entry Types
1. Inherit from `BaseEntry`
2. Add type detection in `PopulateTemplateElements()`
3. Create custom population method

## Troubleshooting

### Template Not Appearing
- Verify the Entry Template field is assigned
- Check that the UXML file path is correct
- Ensure UIDocument has the entry style sheet

### Missing Content
- Check that entry data properties are populated
- Verify the entry is marked as `isSeen = true`
- Confirm the appropriate content methods are being called

### Styling Issues
- Check CSS class names match UXML element names
- Verify style sheet is loaded in UIDocument
- Use browser dev tools for USS debugging

## Performance Notes
- Template cloning is efficient for moderate entry counts
- Consider pooling for very large books (100+ entries)
- CSS animations can be added for enhanced UX