import { useMemo, useRef } from 'react';
import JoditEditor from 'jodit-react';

const RichTextEditor = ({ value, onChange, placeholder = 'Enter description...' }) => {
  const editorRef = useRef(null);

  const config = useMemo(
    () => ({
      readonly: false,
      placeholder,
      height: 220,
      toolbarAdaptive: false,
      buttons:
        'bold,italic,underline,|,ul,ol,|,outdent,indent,|,font,fontsize,brush,|,align,|,link,table,|,undo,redo,|,hr,eraser,fullsize',
      removeButtons: ['about'],
      askBeforePasteHTML: false,
      askBeforePasteFromWord: false,
      defaultMode: 1,
      enter: 'P',
    }),
    [placeholder]
  );

  return (
    <JoditEditor
      ref={editorRef}
      value={value || ''}
      config={config}
      onBlur={(newContent) => onChange?.(newContent)}
    />
  );
};

export default RichTextEditor;

