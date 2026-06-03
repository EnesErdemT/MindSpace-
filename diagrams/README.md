# MindSpace Diyagramları

Bu klasör, MindSpace projesinin tüm mimari ve tasarım diyagramlarını içerir.

## 📋 Diyagram Listesi

1. **1-clean-architecture.md** - Clean Architecture Katmanları
2. **2-entity-relationship.md** - Entity Relationship Diagram (ERD)
3. **3-sequence-post-creation.md** - Post Oluşturma Sequence Diagram
4. **4-data-flow.md** - Veri Akışı (Senkron, Asenkron, Real-Time)
5. **5-deployment-architecture.md** - Deployment Architecture
6. **6-use-case.md** - Use Case Diagram

## 🎨 Mermaid Diyagramları Nasıl Görüntülenir?

### Yöntem 1: GitHub/GitLab (Otomatik Render)
- Dosyaları GitHub veya GitLab'a push edin
- Mermaid diyagramları otomatik olarak render edilir
- En kolay yöntem! ✅

### Yöntem 2: VS Code Extension
1. VS Code'da **Markdown Preview Mermaid Support** extension'ını yükleyin
2. Markdown dosyasını açın
3. `Ctrl+Shift+V` ile preview açın
4. Diyagramlar otomatik render edilir

### Yöntem 3: Online Mermaid Editor
1. https://mermaid.live/ adresine gidin
2. Mermaid kodunu kopyalayıp yapıştırın
3. PNG/SVG olarak export edin

### Yöntem 4: Mermaid CLI (Terminal)
```bash
# Mermaid CLI kurulumu
npm install -g @mermaid-js/mermaid-cli

# PNG'ye dönüştürme
mmdc -i 1-clean-architecture.md -o 1-clean-architecture.png

# Tüm diyagramları dönüştürme
mmdc -i 1-clean-architecture.md -o ../images/clean-architecture.png
mmdc -i 2-entity-relationship.md -o ../images/entity-relationship.png
mmdc -i 3-sequence-post-creation.md -o ../images/sequence-post-creation.png
mmdc -i 4-data-flow.md -o ../images/data-flow.png
mmdc -i 5-deployment-architecture.md -o ../images/deployment-architecture.png
mmdc -i 6-use-case.md -o ../images/use-case.png
```

## 📝 Markdown'a Nasıl Eklenir?

### Direkt Mermaid Kodu (GitHub/GitLab)
```markdown
## 3.2.1 Clean Architecture

```mermaid
graph TB
    ...
```
```

### PNG/SVG Olarak (Tez için önerilen)
```markdown
## 3.2.1 Clean Architecture

![Figura 3.1: Clean Architecture Katmanları](./images/clean-architecture.png)
*Figura 3.1: MindSpace platformunun Clean Architecture katman yapısı*
```

## 🖼️ PNG'ye Dönüştürme (Tez için)

Tezinizde kullanmak için PNG formatına dönüştürmeniz gerekebilir:

### Otomatik Dönüştürme Script (PowerShell)
```powershell
# convert-diagrams.ps1
$diagrams = @(
    "1-clean-architecture",
    "2-entity-relationship",
    "3-sequence-post-creation",
    "4-data-flow",
    "5-deployment-architecture",
    "6-use-case"
)

foreach ($diagram in $diagrams) {
    Write-Host "Converting $diagram..."
    mmdc -i "$diagram.md" -o "../images/$diagram.png" -w 1920 -H 1080
}

Write-Host "✅ All diagrams converted!"
```

Çalıştırma:
```powershell
cd "c:\Users\enser\Desktop\ANUL3\Lucrare de Licenta\MindSpace\diagrams"
.\convert-diagrams.ps1
```

## 🎯 Önerilen Workflow

1. **Geliştirme Aşaması**: Mermaid kodlarını düzenle
2. **Preview**: VS Code extension ile kontrol et
3. **Export**: PNG'ye dönüştür (tez için)
4. **Embed**: Markdown'a PNG olarak ekle

## 📐 Diyagram Boyutları (Tez için)

- **Genişlik**: 1920px (yüksek çözünürlük)
- **Yükseklik**: Otomatik (aspect ratio korunur)
- **Format**: PNG (baskı kalitesi)
- **DPI**: 300 (akademik standart)

## 🔧 Troubleshooting

### Mermaid render edilmiyor
- VS Code extension yüklü mü kontrol edin
- GitHub'da `.md` uzantısı doğru mu kontrol edin
- Syntax hatası var mı kontrol edin

### PNG kalitesi düşük
- `mmdc` komutunda `-w 1920` parametresini kullanın
- `-b transparent` ile arka plan şeffaf olur

### Türkçe karakterler bozuk
- UTF-8 encoding kullanın
- `mmdc -i input.md -o output.png -c config.json`
- config.json'da font ayarları yapın

## 📚 Kaynaklar

- **Mermaid Docs**: https://mermaid.js.org/
- **Mermaid Live Editor**: https://mermaid.live/
- **Mermaid CLI**: https://github.com/mermaid-js/mermaid-cli
- **VS Code Extension**: https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid

---

**Not**: Tüm diyagramlar MindSpace projesinin gerçek mimarisini yansıtmaktadır.
