namespace Oxpecker.Solid

open Fable.Core

module Aria =

    type HtmlTag with
        // ARIA role
        /// Assigns the element a WAI-ARIA semantic role for assistive technology. Use a valid role token such as `button`, `dialog`, `navigation`, or `tab`; prefer a native HTML element when it provides the required semantics.
        [<Erase>]
        member this.role
            with set (_: string) = ()
        // ARIA state, property, and relationship attributes.
        /// ID of the active child in a composite widget when DOM focus remains on the widget.
        [<Erase>]
        member this.ariaActiveDescendant
            with set (_: string) = ()
        /// Whether assistive technology should announce the entire live region when it changes.
        [<Erase>]
        member this.ariaAtomic
            with set (_: bool) = ()
        /// Describes autocomplete suggestions for a combobox. Values: `none`, `inline`, `list`, or `both`.
        [<Erase>]
        member this.ariaAutoComplete
            with set (_: string) = ()
        /// Braille-specific accessible name when the regular accessible name is not suitable for braille output.
        [<Erase>]
        member this.ariaBrailleLabel
            with set (_: string) = ()
        /// Braille-specific description of this element's role; use only when the standard role name is insufficient.
        [<Erase>]
        member this.ariaBrailleRoleDescription
            with set (_: string) = ()
        /// Whether the element or its subtree is being updated. `true` can defer announcements until updates finish.
        [<Erase>]
        member this.ariaBusy
            with set (_: bool) = ()
        /// Checked state of a checkable control. Values: `true`, `false`, or `mixed` when a partial state is supported.
        [<Erase>]
        member this.ariaChecked
            with set (_: string) = ()
        /// Total number of columns in a table, grid, or treegrid; use `-1` when the total is unknown.
        [<Erase>]
        member this.ariaColCount
            with set (_: int) = ()
        /// One-based position of a column or grid cell within its row.
        [<Erase>]
        member this.ariaColIndex
            with set (_: int) = ()
        /// Human-readable alternative to the numeric column index when that index is not meaningful to users.
        [<Erase>]
        member this.ariaColIndexText
            with set (_: string) = ()
        /// Space-separated IDs of elements whose content or behavior this element controls.
        [<Erase>]
        member this.ariaControls
            with set (_: string) = ()
        /// Identifies the current item in a related set. Values: `page`, `step`, `location`, `date`, `time`, `true`, or `false`.
        [<Erase>]
        member this.ariaCurrent
            with set (_: string) = ()
        /// Space-separated IDs of elements that provide an additional description.
        [<Erase>]
        member this.ariaDescribedBy
            with set (_: string) = ()
        /// Text that provides an accessible description of the element.
        [<Erase>]
        member this.ariaDescription
            with set (_: string) = ()
        /// ID of an element that provides more detailed information about this element.
        [<Erase>]
        member this.ariaDetails
            with set (_: string) = ()
        /// Whether the element is perceivable but unavailable for interaction; this does not disable behavior by itself.
        [<Erase>]
        member this.ariaDisabled
            with set (_: bool) = ()
        /// ID of the element containing the error message for this element.
        [<Erase>]
        member this.ariaErrorMessage
            with set (_: string) = ()
        /// Whether this element, or the content it controls, is expanded.
        [<Erase>]
        member this.ariaExpanded
            with set (_: bool) = ()
        /// Space-separated IDs identifying the next element or elements in an alternate reading order.
        [<Erase>]
        member this.ariaFlowTo
            with set (_: string) = ()
        /// Type of popup this element can open. Values: `false`, `true` (equivalent to `menu`), `menu`, `listbox`, `tree`, `grid`, or `dialog`.
        [<Erase>]
        member this.ariaHasPopup
            with set (_: string) = ()
        /// Whether the element is exposed in the accessibility tree. Values: `true` or `false`; do not hide focusable content.
        [<Erase>]
        member this.ariaHidden
            with set (_: bool) = ()
        /// Whether the value is invalid. Values: `false`, `true`, `grammar`, or `spelling`.
        [<Erase>]
        member this.ariaInvalid
            with set (_: string) = ()
        /// Keyboard shortcuts that can activate or focus this element, written as space-separated key combinations.
        [<Erase>]
        member this.ariaKeyShortcuts
            with set (_: string) = ()
        /// Accessible name as a plain text string.
        [<Erase>]
        member this.ariaLabel
            with set (_: string) = ()
        /// Space-separated IDs of elements that provide this element's accessible name.
        [<Erase>]
        member this.ariaLabelledBy
            with set (_: string) = ()
        /// Hierarchical level of the element, such as a heading or tree item; use a positive integer.
        [<Erase>]
        member this.ariaLevel
            with set (_: int) = ()
        /// How updates to this region are announced. Values: `off`, `polite`, or `assertive`.
        [<Erase>]
        member this.ariaLive
            with set (_: string) = ()
        /// Whether this dialog behaves as modal and makes the rest of the interface unavailable.
        [<Erase>]
        member this.ariaModal
            with set (_: bool) = ()
        /// Whether a textbox accepts multiple lines of text.
        [<Erase>]
        member this.ariaMultiLine
            with set (_: bool) = ()
        /// Whether more than one item in this composite widget can be selected.
        [<Erase>]
        member this.ariaMultiSelectable
            with set (_: bool) = ()
        /// Orientation of the widget. Values: `horizontal`, `vertical`, or `undefined`.
        [<Erase>]
        member this.ariaOrientation
            with set (_: string) = ()
        /// Space-separated IDs of elements treated as owned children when DOM structure alone does not express the relationship.
        [<Erase>]
        member this.ariaOwns
            with set (_: string) = ()
        /// Short hint describing the expected value; it is not a replacement for an accessible name.
        [<Erase>]
        member this.ariaPlaceholder
            with set (_: string) = ()
        /// One-based position of this item within its current set.
        [<Erase>]
        member this.ariaPosInSet
            with set (_: int) = ()
        /// Toggle-button state. Values: `true`, `false`, or `mixed`.
        [<Erase>]
        member this.ariaPressed
            with set (_: string) = ()
        /// Whether the element's value can be changed. Values: `true` or `false`.
        [<Erase>]
        member this.ariaReadOnly
            with set (_: string) = ()
        /// Types of live-region changes to announce: `additions`, `removals`, `text`, `all`, or space-separated combinations.
        [<Erase>]
        member this.ariaRelevant
            with set (_: string) = ()
        /// Communicates that a value is required; it does not enforce validation by itself.
        [<Erase>]
        member this.ariaRequired
            with set (_: bool) = ()
        /// Human-readable description of the element's role; use only when the standard role does not explain it adequately.
        [<Erase>]
        member this.ariaRoleDescription
            with set (_: string) = ()
        /// Total number of rows in a table, grid, or treegrid; use `-1` when the total is unknown.
        [<Erase>]
        member this.ariaRowCount
            with set (_: int) = ()
        /// One-based position of a row or grid cell within its table or grid.
        [<Erase>]
        member this.ariaRowIndex
            with set (_: int) = ()
        /// Human-readable alternative to the numeric row index when that index is not meaningful to users.
        [<Erase>]
        member this.ariaRowIndexText
            with set (_: string) = ()
        /// Number of rows occupied by this cell in a table or grid; use a positive integer.
        [<Erase>]
        member this.ariaRowSpan
            with set (_: int) = ()
        /// Whether this selectable item is currently selected.
        [<Erase>]
        member this.ariaSelected
            with set (_: bool) = ()
        /// Total number of items in the current set; use `-1` when the total is unknown.
        [<Erase>]
        member this.ariaSetSize
            with set (_: int) = ()
        /// Sort order of the items in a table or grid. Values: `ascending`, `descending`, `none`, or `other`.
        [<Erase>]
        member this.ariaSort
            with set (_: string) = ()
        /// Maximum numeric value for a range widget.
        [<Erase>]
        member this.ariaValueMax
            with set (_: string) = ()
        /// Minimum numeric value for a range widget.
        [<Erase>]
        member this.ariaValueMin
            with set (_: string) = ()
        /// Current numeric value for a range widget, within its minimum and maximum; omit when indeterminate.
        [<Erase>]
        member this.ariaValueNow
            with set (_: string) = ()
        /// Human-readable text for the current value of a range widget.
        [<Erase>]
        member this.ariaValueText
            with set (_: string) = ()
