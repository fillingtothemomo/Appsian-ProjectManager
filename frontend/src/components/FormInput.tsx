import React from "react";
export default function FormInput(props: {
  label: string;
  value: string;
  type?: string;
  onChange: (v:string)=>void;
  placeholder?: string;
}) {
  return (
    <label className="block">
      <div className="text-sm font-medium mb-1">{props.label}</div>
      <input
        value={props.value}
        onChange={(e)=>props.onChange(e.target.value)}
        type={props.type ?? "text"}
        placeholder={props.placeholder}
        className="input"
      />
    </label>
  );
}
