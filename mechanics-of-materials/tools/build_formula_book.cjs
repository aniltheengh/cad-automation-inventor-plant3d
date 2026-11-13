/* Build a self-contained vector-math HTML book and a print PDF from the checked Markdown.
   Install tools/package.json dependencies; set CHROME_PATH when Chrome is not in its default location. */
const fs=require('node:fs'), path=require('node:path');
const {pathToFileURL}=require('node:url');
const {mathjax}=require('mathjax-full/js/mathjax.js');
const {TeX}=require('mathjax-full/js/input/tex.js');
const {SVG}=require('mathjax-full/js/output/svg.js');
const {liteAdaptor}=require('mathjax-full/js/adaptors/liteAdaptor.js');
const {RegisterHTMLHandler}=require('mathjax-full/js/handlers/html.js');
const {AllPackages}=require('mathjax-full/js/input/tex/AllPackages.js');
const MarkdownIt=require('markdown-it');
const {chromium}=require('playwright-core');
const root=path.resolve(__dirname,'..');
const out=path.join(root,'formula-book');
fs.mkdirSync(out,{recursive:true});
const adapter=liteAdaptor(); RegisterHTMLHandler(adapter);
const doc=mathjax.document('',{InputJax:new TeX({packages:AllPackages}),OutputJax:new SVG({fontCache:'none'})});
const md=new MarkdownIt({html:true,typographer:false});
const BT=String.fromCharCode(96);
const escape=s=>s.replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;');
let parsed=0;const errors=[];
function equation(tex,display=false){
 parsed++;
 try{
  const node=doc.convert(tex.trim(),{display});
  let result=adapter.outerHTML(node);
  if(result.includes('data-mml-node="merror"'))throw Error('MathJax reported merror');
  return '<span class="math '+(display?'display':'inline')+'" role="math" aria-label="'+escape(tex)+'">'+result+'</span>';
 }catch(e){errors.push({tex,error:String(e)});return '<strong class="math-error">'+escape(tex)+'</strong>';}
}
function splitEquations(tex){
 // Split only top-level comma + spacing separators, not commas inside fractions or functions.
 const result=[];let level=0,start=0;
 for(let i=0;i<tex.length;i++){
  if(tex[i]==='{')level++;if(tex[i]==='}')level--;
  if(level===0&&tex[i]===','){
   const m=tex.slice(i).match(/^,\s*\\q(?:uad|quad)\s*/);
   if(m){result.push(tex.slice(start,i));i+=m[0].length-1;start=i+1;}
  }
 }
 result.push(tex.slice(start));
 return result.map(x=>x.trim().replace(/[,.]$/,'')).filter(Boolean);
}
function render(source){
 const saved=[];const token=html=>{saved.push(html);return 'MATHPLACEHOLDER'+(saved.length-1)+'END';};
 let s=source.replace(new RegExp(BT.repeat(3)+'math\\s*\\n([\\s\\S]*?)\\n'+BT.repeat(3),'g'),(_,tex)=>'\n\n'+token('<div class="equation-group">'+splitEquations(tex).map(t=>equation(t,true)).join('')+'</div>')+'\n\n');
 s=s.replace(/\$\$\s*\n([\s\S]*?)\n\s*\$\$/g,(_,tex)=>'\n\n'+token('<div class="equation-group">'+splitEquations(tex).map(t=>equation(t,true)).join('')+'</div>')+'\n\n');
 s=s.replace(/\$\x60([^\n]*?)\x60\$|\$([^$\n]+)\$/g,(_,a,b)=>token(equation(a||b,false)));
 let html=md.render(s);
 html=html.replace(/<p>(MATHPLACEHOLDER\d+END)<\/p>/g,'$1');
 html=html.replace(/MATHPLACEHOLDER(\d+)END/g,(_,n)=>saved[+n]);
 html=html.replace(/<div class="equation-group">([\s\S]*?)<\/div>/g,(all,inner)=>{const widths=[...inner.matchAll(/<svg[^>]*width="([\d.]+)ex"/g)].map(m=>+m[1]);return widths.length>1&&widths.every(w=>w<33)?all.replace('equation-group','equation-group two-col'):all;});
 return html;
}
const read=p=>fs.readFileSync(path.join(root,p),'utf8').replace(/\r\n/g,'\n');
const reference=read('formula-reference.md');
const sections=reference.split(/(?=^## )/m).filter(s=>/^## I+-\d+\./.test(s));
if(sections.length!==26)throw Error('Expected 26 formula topics');
const definitions=JSON.parse(read('tools/formula-definitions.json'));
const chapterFiles=fs.readdirSync(path.join(root,'chapters')).filter(f=>f.endsWith('.md')).sort();
if(chapterFiles.length!==26)throw Error('Expected 26 chapters');
const github='https://github.com/aniltheengh/cad-automation-inventor-plant3d/blob/main/mechanics-of-materials/';
const topics=sections.map((s,i)=>{
 const heading=s.match(/^## (.+)/)[1]; let body=s.replace(/^## .+\n/,'');
 body=body.replace(/\[Chapter \d+\]\(chapters\/[^)]+\)\.?/gi,'').trim();
 const chapter=read('chapters/'+chapterFiles[i]);
 let example=chapter.match(/## Worked example\s*\n([\s\S]*?)(?=\n## |$)/)?.[1];
 if(!example)example=chapter.split('\n').filter(l=>l.startsWith('**Example:**')).join('\n\n');
 let limits=chapter.match(/## (?:Transition(?: and limitations)?|Scope|Intermediate checks)\s*\n([\s\S]*?)(?=\n## |$)/)?.[1]||'Use the assumptions stated beside the formulas and the linked chapter.';
 // Make reference links usable from both the standalone HTML and PDF.
 body=body.replace(/\]\(chapters\/([^)]+)\)/g,']('+github+'chapters/$1)');
 return '<section class="topic" id="topic-'+(i+1)+'"><div class="eyebrow">'+(i<12?'MECHANICS OF MATERIALS I':'MECHANICS OF MATERIALS II')+' / '+String(i+1).padStart(2,'0')+'</div><h1>'+escape(heading.replace(/^I+-\d+\.\s*/,''))+'</h1><div class="symbols"><b>Symbols on this sheet</b><p>'+escape(definitions[i])+'</p></div>'+render(body)+'<div class="example"><h2>Worked check</h2>'+render(example||'See the chapter for the worked calculation.')+'</div><div class="limits"><h2>When to use this model</h2>'+render(limits)+'<p class="chapter-link"><a href="'+github+'chapters/'+chapterFiles[i]+'">Open the full chapter and practice question</a></p></div></section>';
});
const contents=sections.map((s,i)=>'<a href="#topic-'+(i+1)+'"><span>'+String(i+1).padStart(2,'0')+'</span>'+escape(s.match(/^## I+-\d+\.\s*(.*)/)[1])+'</a>').join('');
const css=[
'@page{size:A4;margin:17mm 17mm 18mm;}',
'*{box-sizing:border-box} body{margin:0;color:#182b3a;font-family:Arial,Helvetica,sans-serif;font-size:10.3pt;line-height:1.32;background:#fff}',
'a{color:#116d85;text-decoration:none} p{margin:7px 0 10px} h1{font-size:24pt;line-height:1.12;color:#103e52;margin:8px 0 12px;font-weight:700} h2{font-size:13pt;color:#116d85;margin:16px 0 7px;break-after:avoid} h3{font-size:11pt;margin:12px 0 6px}',
'.cover{padding-top:35mm;min-height:240mm;break-after:page}.cover h1{font-size:41pt;max-width:570px}.cover .subtitle{font-size:20pt;line-height:1.4;color:#476272}.cover .rule{height:6px;background:#d3a344;width:75px;margin:26px 0}.eyebrow{font-size:8.5pt;font-weight:700;letter-spacing:1.8px;color:#53737f}',
'.toc{break-after:page}.toc-list{columns:2;column-gap:26px}.toc-list a{display:block;break-inside:avoid;padding:8px 0;border-bottom:1px solid #d8e3e8;font-size:10pt}.toc-list span{display:inline-block;width:27px;font-weight:bold;color:#103e52}',
'.guide{break-after:page}.topic{break-before:page}.symbols{background:#edf4f7;border-left:4px solid #116d85;padding:7px 10px;font-size:9.3pt;margin-bottom:9px;break-inside:avoid}.symbols p{margin-bottom:0}.example{background:#f6f7ef;border-left:4px solid #a2a86d;padding:7px 10px;margin-top:10px;break-inside:avoid}.example h2{margin-top:0}.limits{margin-top:9px;border-top:1px solid #c6d7df;padding-top:4px;break-inside:avoid}.limits p{font-size:9.5pt}',
'.two-col{display:grid;grid-template-columns:1fr 1fr;column-gap:16px;align-items:center}.equation-group{margin:6px 0 8px;padding:6px 10px;background:#f8fafb;border:1px solid #e0e9ed;break-inside:avoid}.math.display{display:block;font-size:12.2pt;margin:3px 0;line-height:1.4;break-inside:avoid}.math.inline{font-size:1em} mjx-container[jax="SVG"]{display:inline-block;line-height:0} mjx-container svg{overflow:visible}.math.display mjx-container{display:block!important;text-align:left!important;margin:0!important}.math.inline svg{vertical-align:middle}.chapter-link{font-size:8.8pt;margin-top:8px}',
'table{border-collapse:collapse;width:100%;margin:10px 0 14px;font-size:9.2pt;break-inside:avoid}th,td{border-bottom:1px solid #ccdce4;padding:8px 7px;text-align:left;vertical-align:middle}th{background:#eaf2f5;color:#103e52}tr:nth-child(even){background:#f8fafb}ul,ol{padding-left:23px}li{margin:4px 0}strong{font-weight:700} .small{font-size:9pt;color:#506874}',
'@media screen{body{background:#edf2f5;padding:24px}.cover,.toc,.guide,.topic,.end{max-width:840px;margin:0 auto 24px;background:white;padding:45px 56px;box-shadow:0 3px 18px #19394d15}.math.display{overflow-x:auto;padding:4px 0}.cover{min-height:0}.toc-list{columns:2}}',
'@media print{.topic h1{font-size:23pt} .math.display svg{max-width:100%;height:auto} .end{break-before:page}}'
].join('\n');
const guide=render('# How to use the book\n\nRead the symbols and the model assumptions before substituting numbers. The boxed worked check lets you confirm units and magnitudes. This is a reference for the listed two-course syllabus, not a complete structural design standard.\n\n## Consistent units\n\n| Quantity | SI | Convenient part calculations |\n|---|---|---|\n| Force | N | N |\n| Length | m | mm |\n| Stress / modulus | Pa | MPa = N/mm² |\n| Moment / torque | N m | N mm |\n| Area moment / torsion constant | m⁴ | mm⁴ |\n| Energy | J | N mm; divide by 1000 for J |\n| Strain | dimensionless | microstrain = 10⁻⁶ |\n| Rotation | radians | radians |\n\n## Sign and model conventions\n\n- Normal stress is tensile-positive; compression is negative, except where a compressive load magnitude is explicitly used.\n- Beam coordinate x increases right; load w is downward-positive; M is sagging-positive; v is upward-positive.\n- Tables give positive magnitudes where stated. Do not transfer their sign into a different convention without checking.\n- Engineering shear strain is twice tensor shear strain.\n- J for circular torsion equals the polar area moment; Jt for a noncircular section generally does not.\n- Use all three principal stresses in 3D failure checks, including zero for plane stress.\n\n## A reliable calculation\n\nSketch the geometry and supports. Find internal loads. Select the governing model. Keep units consistent. Calculate stress and deformation. Check equilibrium and a limiting case. For real design, apply the relevant material data, load combinations and standards.\n\n## Scope and revision\n\n26 topics cover Mechanics of Materials I and II. Version: 13 November 2026, reviewed formula-book edition. Formula source, detailed chapters, books and correction notes are available in the repository.');
const html='<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Mechanics of Materials I & II - Formula Book</title><style>'+css+'</style></head><body><section class="cover"><div class="eyebrow">ENGINEERING REFERENCE / REVIEWED EDITION</div><h1>Mechanics<br>of Materials</h1><div class="subtitle">I & II<br>Formula Book</div><div class="rule"></div><p>Strength of materials, from axial loading to finite elements.</p><p>26 topic sheets with equations, symbols, model limits and worked checks.</p><p class="small">13 November 2026<br>Companion to cad-automation-inventor-plant3d</p></section><section class="toc"><div class="eyebrow">FIND A TOPIC</div><h1>Contents</h1><div class="toc-list">'+contents+'</div></section><section class="guide">'+guide+'</section>'+topics.join('')+'<section class="end"><div class="eyebrow">READING AND CHECKING</div><h1>Sources and verification</h1><p>These formula sheets are original study notes. The repository contains David Roylance’s <i>Modules in Mechanics of Materials</i>, MIT OpenCourseWare, under its separate CC BY-NC-SA licence. The original textbook is not reproduced inside this formula book.</p><p><a href="'+github+'books/README.md">Book attribution and further reading</a></p><p><a href="'+github+'formula-audit.md">Audit scope, corrections and validation</a></p><p>The checks detect equation syntax errors, numerical-example regressions, broken internal links and layout overflow. They are not independent engineering certification. Assumptions and physical applicability still need checking for a real component.</p><p>Build sources are included in mechanics-of-materials/tools. Both the HTML and PDF are generated from the corrected Markdown reference and chapters. The HTML includes its vector equations and does not need an internet connection.</p></section></body></html>';
// Also parse all mathematics in the chapter sources, not merely those used on the printed sheets.
for(const f of chapterFiles)render(read('chapters/'+f));
render(read('symbols-and-units.md'));
if(errors.length){fs.writeFileSync(path.join(out,'math-errors.json'),JSON.stringify(errors,null,2));throw Error(errors.length+' equation errors');}
const htmlPath=path.join(out,'Mechanics_of_Materials_Formula_Book.html');
fs.writeFileSync(htmlPath,html,'utf8');
(async()=>{
 const browser=await chromium.launch({executablePath:process.env.CHROME_PATH||'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true});
 const page=await browser.newPage({viewport:{width:1100,height:900}});
 await page.goto(pathToFileURL(htmlPath).href);
 await page.emulateMedia({media:'print'});
 await page.evaluate(()=>document.fonts.ready);
 // 176mm printable width is about 665px at 96dpi.
 const report=await page.evaluate(()=>{
  const bad=[...document.querySelectorAll('.equation-group svg')].filter(e=>e.getBoundingClientRect().width>640).map(e=>({width:e.getBoundingClientRect().width,tex:e.closest('[aria-label]')?.getAttribute('aria-label')}));
  return {displayEquations:document.querySelectorAll('.math.display').length,inlineEquations:document.querySelectorAll('.math.inline').length,wideEquations:bad,mathErrors:document.querySelectorAll('[data-mml-node="merror"]').length};
 });
 if(report.wideEquations.length)throw Error('Equations wider than printable width: '+JSON.stringify(report.wideEquations));
 const pdfPath=path.join(out,'Mechanics_of_Materials_Formula_Book.pdf');
 await page.pdf({path:pdfPath,format:'A4',printBackground:true,preferCSSPageSize:true,displayHeaderFooter:true,headerTemplate:'<div></div>',footerTemplate:'<div style="font-size:8px;color:#607580;width:100%;padding:0 17mm;display:flex;justify-content:space-between;"><span>MECHANICS OF MATERIALS I &amp; II · FORMULA BOOK</span><span class="pageNumber"></span></div>',tagged:true,outline:true});
 await browser.close();
 fs.writeFileSync(path.join(out,'validation.json'),JSON.stringify({parsedExpressions:parsed,...report},null,2)+'\n');
 console.log(JSON.stringify({parsedExpressions:parsed,...report,pdfPath}));
})().catch(e=>{console.error(e);process.exitCode=1});
