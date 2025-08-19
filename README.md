# PDF Font Inspector

A lightweight .NET 8 console application that inspects PDF documents to:
- **List all embedded fonts** (including Type3 / custom fonts)
- **Detect whether each page contains real, copyable text**
- **Count extractable characters per page**

---

## Features

- Lists every unique font (BaseFont + Type)
- Detects whether fonts are subsetted
- Checks if text is extractable per page (`YES`/`NO` + character count)
- Safe for malformed PDFs (handles page-level exceptions)

---

## Installation

Clone this repository and install dependencies:

```bash
git clone https://github.com/<your-username>/PdfFontInspector.git
cd PdfFontInspector
dotnet restore

## Usage

```bash
dotnet run -- "..\Test\test.pdf"