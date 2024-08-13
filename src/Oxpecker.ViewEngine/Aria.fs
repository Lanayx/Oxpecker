namespace Oxpecker.ViewEngine

module Aria =

    type HtmlTag with
        // aria role
        member this.role
            with set value = this.attr("role", value) |> ignore
        // aria attributes
        member this.ariaActiveDescendant
            with set value = this.attr("aria-activedescendant", value) |> ignore
        member this.ariaAtomic
            with set value = this.attr("aria-atomic", (if value then "true" else "false")) |> ignore
        member this.ariaAutoComplete
            with set value = this.attr("aria-autocomplete", value) |> ignore
        member this.ariaBrailleLabel
            with set value = this.attr("aria-braillelabel", value) |> ignore
        member this.ariaBrailleRoleDescription
            with set value = this.attr("aria-brailleroledescription", value) |> ignore
        member this.ariaBusy
            with set value = this.attr("aria-busy", (if value then "true" else "false")) |> ignore
        member this.ariaChecked
            with set value = this.attr("aria-checked", value) |> ignore
        member this.ariaColCount
            with set (value: int) = this.attr("aria-colcount", string value) |> ignore
        member this.ariaColIndex
            with set (value: int) = this.attr("aria-colindex", string value) |> ignore
        member this.ariaColIndexText
            with set value = this.attr("aria-colindextext", value) |> ignore
        member this.ariaControls
            with set value = this.attr("aria-controls", value) |> ignore
        member this.ariaCurrent
            with set value = this.attr("aria-current", value) |> ignore
        member this.ariaDescribedBy
            with set value = this.attr("aria-describedby", value) |> ignore
        member this.ariaDescription
            with set value = this.attr("aria-description", value) |> ignore
        member this.ariaDetails
            with set value = this.attr("aria-details", value) |> ignore
        member this.ariaDisabled
            with set value = this.attr("aria-disabled", (if value then "true" else "false")) |> ignore
        member this.ariaErrorMessage
            with set value = this.attr("aria-errormessage", value) |> ignore
        member this.ariaExpanded
            with set value = this.attr("aria-expanded", (if value then "true" else "false")) |> ignore
        member this.ariaFlowTo
            with set value = this.attr("aria-flowto", value) |> ignore
        member this.ariaHasPopup
            with set value = this.attr("aria-haspopup", value) |> ignore
        member this.ariaHidden
            with set value = this.attr("aria-hidden", (if value then "true" else "false")) |> ignore
        member this.ariaInvalid
            with set value = this.attr("aria-invalid", value) |> ignore
        member this.ariaKeyShortcuts
            with set value = this.attr("aria-keyshortcuts", value) |> ignore
        member this.ariaLabel
            with set value = this.attr("aria-label", value) |> ignore
        member this.ariaLabelledBy
            with set value = this.attr("aria-labelledby", value) |> ignore
        member this.ariaLevel
            with set (value: int) = this.attr("aria-level", string value) |> ignore
        member this.ariaLive
            with set value = this.attr("aria-live", value) |> ignore
        member this.ariaModal
            with set value = this.attr("aria-modal", (if value then "true" else "false")) |> ignore
        member this.ariaMultiLine
            with set value = this.attr("aria-multiline", (if value then "true" else "false")) |> ignore
        member this.ariaMultiSelectable
            with set value = this.attr("aria-multiselectable", (if value then "true" else "false")) |> ignore
        member this.ariaOrientation
            with set value = this.attr("aria-orientation", value) |> ignore
        member this.ariaOwns
            with set value = this.attr("aria-owns", value) |> ignore
        member this.ariaPlaceholder
            with set value = this.attr("aria-placeholder", value) |> ignore
        member this.ariaPosInSet
            with set (value: int) = this.attr("aria-posinset", string value) |> ignore
        member this.ariaPressed
            with set value = this.attr("aria-pressed", value) |> ignore
        member this.ariaReadOnly
            with set value = this.attr("aria-readonly", value) |> ignore
        member this.ariaRelevant
            with set value = this.attr("aria-relevant", value) |> ignore
        member this.ariaRequired
            with set value = this.attr("aria-required", (if value then "true" else "false")) |> ignore
        member this.ariaRoleDescription
            with set value = this.attr("aria-roledescription", value) |> ignore
        member this.ariaRowCount
            with set (value: int) = this.attr("aria-rowcount", string value) |> ignore
        member this.ariaRowIndex
            with set (value: int) = this.attr("aria-rowindex", string value) |> ignore
        member this.ariaRowIndexText
            with set value = this.attr("aria-rowindextext", value) |> ignore
        member this.ariaRowSpan
            with set (value: int) = this.attr("aria-rowspan", string value) |> ignore
        member this.ariaSelected
            with set value = this.attr("aria-selected", (if value then "true" else "false")) |> ignore
        member this.ariaSetSize
            with set (value: int) = this.attr("aria-setsize", string value) |> ignore
        member this.ariaSort
            with set value = this.attr("aria-sort", value) |> ignore
        member this.ariaValueMax
            with set value = this.attr("aria-valuemax", value) |> ignore
        member this.ariaValueMin
            with set value = this.attr("aria-valuemin", value) |> ignore
        member this.ariaValueNow
            with set value = this.attr("aria-valuenow", value) |> ignore
        member this.ariaValueText
            with set value = this.attr("aria-valuetext", value) |> ignore
