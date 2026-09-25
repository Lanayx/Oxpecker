namespace Oxpecker.ViewEngine

module Aria =

    type HtmlTag with
        // aria role
        /// Assigns a WAI-ARIA semantic role to the element. Use a valid role token such as `button`, `dialog`, `navigation`, or `tab`; prefer native HTML semantics when available.
        member this.role
            with set (value: string | null) = this.attr("role", value) |> ignore
        // aria attributes
        /// ID of the active child in a composite widget when DOM focus remains on the widget.
        member this.ariaActiveDescendant
            with set (value: string | null) = this.attr("aria-activedescendant", value) |> ignore
        /// Whether assistive technology should announce the entire live region when it changes.
        member this.ariaAtomic
            with set (value: bool) = this.attr("aria-atomic", (if value then "true" else "false")) |> ignore
        /// Autocomplete behavior for a combobox: `none`, `inline`, `list`, or `both`.
        member this.ariaAutoComplete
            with set (value: string | null) = this.attr("aria-autocomplete", value) |> ignore
        /// Braille-specific accessible name when the regular name is unsuitable for braille output.
        member this.ariaBrailleLabel
            with set (value: string | null) = this.attr("aria-braillelabel", value) |> ignore
        /// Braille-specific role description, for cases where the standard role name is insufficient.
        member this.ariaBrailleRoleDescription
            with set (value: string | null) = this.attr("aria-brailleroledescription", value) |> ignore
        /// Whether the element or its subtree is being updated; `true` can defer announcements until updates finish.
        member this.ariaBusy
            with set (value: bool) = this.attr("aria-busy", (if value then "true" else "false")) |> ignore
        /// Checked state of a checkable control: `true`, `false`, or `mixed` when supported.
        member this.ariaChecked
            with set (value: string | null) = this.attr("aria-checked", value) |> ignore
        /// Total number of columns in a table, grid, or treegrid; use `-1` when unknown.
        member this.ariaColCount
            with set (value: int) = this.attr("aria-colcount", string value) |> ignore
        /// One-based position of a column or grid cell within its row.
        member this.ariaColIndex
            with set (value: int) = this.attr("aria-colindex", string value) |> ignore
        /// Human-readable alternative to the numeric column index when needed.
        member this.ariaColIndexText
            with set (value: string | null) = this.attr("aria-colindextext", value) |> ignore
        /// Space-separated IDs of elements whose content or behavior this element controls.
        member this.ariaControls
            with set (value: string | null) = this.attr("aria-controls", value) |> ignore
        /// Identifies the current item in a related set: `page`, `step`, `location`, `date`, `time`, `true`, or `false`.
        member this.ariaCurrent
            with set (value: string | null) = this.attr("aria-current", value) |> ignore
        /// Space-separated IDs of elements that provide an additional description.
        member this.ariaDescribedBy
            with set (value: string | null) = this.attr("aria-describedby", value) |> ignore
        /// Text that provides an accessible description of the element.
        member this.ariaDescription
            with set (value: string | null) = this.attr("aria-description", value) |> ignore
        /// Space-separated IDs of elements that provide more detailed information about this element.
        member this.ariaDetails
            with set (value: string | null) = this.attr("aria-details", value) |> ignore
        /// Whether the element is perceivable but unavailable for interaction; this does not disable behavior by itself.
        member this.ariaDisabled
            with set (value: bool) = this.attr("aria-disabled", (if value then "true" else "false")) |> ignore
        /// ID of the element containing the error message for this element.
        member this.ariaErrorMessage
            with set (value: string | null) = this.attr("aria-errormessage", value) |> ignore
        /// Whether this element, or the content it controls, is expanded.
        member this.ariaExpanded
            with set (value: bool) = this.attr("aria-expanded", (if value then "true" else "false")) |> ignore
        /// Space-separated IDs identifying the next element or elements in an alternate reading order.
        member this.ariaFlowTo
            with set (value: string | null) = this.attr("aria-flowto", value) |> ignore
        /// Popup type this element can open: `false`, `true` (equivalent to `menu`), `menu`, `listbox`, `tree`, `grid`, or `dialog`.
        member this.ariaHasPopup
            with set (value: string | null) = this.attr("aria-haspopup", value) |> ignore
        /// Whether the element is exposed in the accessibility tree: `true` or `false`; do not hide focusable content.
        member this.ariaHidden
            with set (value: bool) = this.attr("aria-hidden", (if value then "true" else "false")) |> ignore
        /// Whether the value is invalid: `false`, `true`, `grammar`, or `spelling`.
        member this.ariaInvalid
            with set (value: string | null) = this.attr("aria-invalid", value) |> ignore
        /// Space-separated keyboard shortcuts that can activate or focus the element.
        member this.ariaKeyShortcuts
            with set (value: string | null) = this.attr("aria-keyshortcuts", value) |> ignore
        /// Accessible name provided as plain text.
        member this.ariaLabel
            with set (value: string | null) = this.attr("aria-label", value) |> ignore
        /// Space-separated IDs of elements that provide this element's accessible name.
        member this.ariaLabelledBy
            with set (value: string | null) = this.attr("aria-labelledby", value) |> ignore
        /// Hierarchical level, such as for a heading or tree item; use a positive integer.
        member this.ariaLevel
            with set (value: int) = this.attr("aria-level", string value) |> ignore
        /// How updates to this region are announced: `off`, `polite`, or `assertive`.
        member this.ariaLive
            with set (value: string | null) = this.attr("aria-live", value) |> ignore
        /// Whether a dialog behaves as modal and makes the rest of the interface unavailable.
        member this.ariaModal
            with set (value: bool) = this.attr("aria-modal", (if value then "true" else "false")) |> ignore
        /// Whether a textbox accepts multiple lines of text.
        member this.ariaMultiLine
            with set (value: bool) = this.attr("aria-multiline", (if value then "true" else "false")) |> ignore
        /// Whether multiple items can be selected in this composite widget.
        member this.ariaMultiSelectable
            with set (value: bool) = this.attr("aria-multiselectable", (if value then "true" else "false")) |> ignore
        /// Widget orientation: `horizontal`, `vertical`, or `undefined`.
        member this.ariaOrientation
            with set (value: string | null) = this.attr("aria-orientation", value) |> ignore
        /// Space-separated IDs of elements treated as owned children when the DOM structure does not express the relationship.
        member this.ariaOwns
            with set (value: string | null) = this.attr("aria-owns", value) |> ignore
        /// Short hint describing the expected value; it is not a replacement for an accessible name.
        member this.ariaPlaceholder
            with set (value: string | null) = this.attr("aria-placeholder", value) |> ignore
        /// One-based position of this item within its current set.
        member this.ariaPosInSet
            with set (value: int) = this.attr("aria-posinset", string value) |> ignore
        /// Toggle-button state: `true`, `false`, or `mixed`.
        member this.ariaPressed
            with set (value: string | null) = this.attr("aria-pressed", value) |> ignore
        /// Whether the element value can be changed: `true` or `false`.
        member this.ariaReadOnly
            with set (value: string | null) = this.attr("aria-readonly", value) |> ignore
        /// Live-region changes to announce: `additions`, `removals`, `text`, `all`, or space-separated combinations.
        member this.ariaRelevant
            with set (value: string | null) = this.attr("aria-relevant", value) |> ignore
        /// Communicates that a value is required; it does not enforce validation by itself.
        member this.ariaRequired
            with set (value: bool) = this.attr("aria-required", (if value then "true" else "false")) |> ignore
        /// Human-readable role description; use only when the standard role does not explain it adequately.
        member this.ariaRoleDescription
            with set (value: string | null) = this.attr("aria-roledescription", value) |> ignore
        /// Total number of rows in a table, grid, or treegrid; use `-1` when unknown.
        member this.ariaRowCount
            with set (value: int) = this.attr("aria-rowcount", string value) |> ignore
        /// One-based position of a row or grid cell within its table or grid.
        member this.ariaRowIndex
            with set (value: int) = this.attr("aria-rowindex", string value) |> ignore
        /// Human-readable alternative to the numeric row index when needed.
        member this.ariaRowIndexText
            with set (value: string | null) = this.attr("aria-rowindextext", value) |> ignore
        /// Number of rows occupied by this cell in a table or grid; use a positive integer.
        member this.ariaRowSpan
            with set (value: int) = this.attr("aria-rowspan", string value) |> ignore
        /// Whether this selectable item is currently selected.
        member this.ariaSelected
            with set (value: bool) = this.attr("aria-selected", (if value then "true" else "false")) |> ignore
        /// Total number of items in the current set; use `-1` when unknown.
        member this.ariaSetSize
            with set (value: int) = this.attr("aria-setsize", string value) |> ignore
        /// Sort order of table or grid items: `ascending`, `descending`, `none`, or `other`.
        member this.ariaSort
            with set (value: string | null) = this.attr("aria-sort", value) |> ignore
        /// Maximum numeric value for a range widget.
        member this.ariaValueMax
            with set (value: string | null) = this.attr("aria-valuemax", value) |> ignore
        /// Minimum numeric value for a range widget.
        member this.ariaValueMin
            with set (value: string | null) = this.attr("aria-valuemin", value) |> ignore
        /// Current numeric value for a range widget; omit when the value is indeterminate.
        member this.ariaValueNow
            with set (value: string | null) = this.attr("aria-valuenow", value) |> ignore
        /// Human-readable text for the current value of a range widget.
        member this.ariaValueText
            with set (value: string | null) = this.attr("aria-valuetext", value) |> ignore
