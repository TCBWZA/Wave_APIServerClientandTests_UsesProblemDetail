# Invoice Number Validation - Valid vs Invalid Examples

## ? Valid Invoice Numbers

These invoice numbers **START WITH "INV"** (case-sensitive uppercase):

```
? INV-12345
? INV-TECH001
? INV2024-001
? INVOICE-12345    ? Valid! Starts with "INV"
? INV_ABC_123
? INVoice-2024
? INV
```

**Rule:** Must start with uppercase "INV" (case-sensitive)

**Regex:** `^INV.*`
- `^` = Start of string
- `INV` = Literal characters "INV"
- `.*` = Any characters after (including none)

## ? Invalid Invoice Numbers

These invoice numbers **DO NOT START WITH "INV"**:

```
? ORDER-12345       ? Used in Example 7a
? Invoice-12345     ? Lowercase "i"
? invoice-12345     ? All lowercase
? I-12345          ? Missing "NV"
? IN-12345         ? Missing "V"
? 12345            ? No prefix
? ""               ? Empty (also violates Required)
? null             ? Null (also violates Required)
? " INV-123"       ? Starts with space
? SINV-123         ? Starts with "S"
```

## ?? Common Misconceptions

### Misconception 1: "INVOICE-12345 is invalid"
**Wrong!** ?

"INVOICE-12345" **IS VALID** because:
- It starts with "I"
- Then "N"
- Then "V"
- Pattern `^INV.*` matches: **INV**OICE-12345

### Misconception 2: Case doesn't matter
**Wrong!** ?

The regex `^INV.*` is **case-sensitive**:
- `INV-123` ? Valid
- `Inv-123` ? Invalid
- `inv-123` ? Invalid

### Misconception 3: Spaces before INV are OK
**Wrong!** ?

The `^` anchor means start of string:
- `INV-123` ? Valid
- ` INV-123` ? Invalid (starts with space)

## ?? Example Test Cases

### Valid Cases
```csharp
// All these will pass validation
"INV-001"          ?
"INV-TECH-2024"    ?
"INV123"           ?
"INVOICE-XYZ"      ?
"INVoice123"       ?
```

### Invalid Cases
```csharp
// All these will fail validation
"ORDER-001"        ? "InvoiceNumber must start with 'INV'."
"Invoice-001"      ? "InvoiceNumber must start with 'INV'."
"123-INV"          ? "InvoiceNumber must start with 'INV'."
""                 ? "InvoiceNumber is required." + "must start with 'INV'."
```

## ?? Code Implementation

### Model Validation
```csharp
[Required(ErrorMessage = "InvoiceNumber is required.")]
[RegularExpression(@"^INV.*", ErrorMessage = "InvoiceNumber must start with 'INV'.")]
public string InvoiceNumber { get; set; } = string.Empty;
```

### Testing the Pattern
```csharp
using System.Text.RegularExpressions;

var pattern = @"^INV.*";

// Valid examples
Regex.IsMatch("INV-12345", pattern)      // true
Regex.IsMatch("INVOICE-12345", pattern)  // true
Regex.IsMatch("INV", pattern)            // true

// Invalid examples
Regex.IsMatch("ORDER-12345", pattern)    // false
Regex.IsMatch("Invoice-12345", pattern)  // false
Regex.IsMatch("", pattern)               // false
```

## ?? Recommendations for Examples

### Good Invalid Examples (clear)
```csharp
"ORDER-12345"      // Clearly doesn't start with INV
"PO-12345"         // Purchase Order prefix
"BILL-12345"       // Bill prefix
"123456"           // Just numbers
"invoice-123"      // Lowercase
```

### Avoid as Invalid Examples (confusing)
```csharp
"INVOICE-12345"    // ? Actually valid!
"Invoice-12345"    // Could be mistaken as valid
"INVoice-12345"    // Could be mistaken as valid
```

## ?? Summary

**Remember:**
- `^INV.*` means "starts with uppercase INV"
- "INVOICE-12345" **IS VALID** ?
- "ORDER-12345" **IS INVALID** ?
- Case matters: "INV" ? "Inv" ? "inv"

---

**Updated Examples:**
- Example 7a now uses: `"ORDER-12345"` ? Correctly invalid
- Documentation updated to avoid confusion
