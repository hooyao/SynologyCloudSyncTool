meta:
  id: cloudsync
  endian: be
seq:
  - id: magic_header
    type: header
  - id: meta_data
    type: dict
  - id: raw_data_list
    type: raw_data_list
types:
  header:
    seq:
      - id: magic
        contents: '__CLOUDSYNC_ENC__'
      - id: magic_hash
        size: 32
  dict_dim: {}
  entry_key:
    seq:
      - id: str_tag
        contents: [0x10]
      - id: str_len
        type: u2
      - id: str_content
        size: str_len
        type: str
        encoding: ascii
  lv_int:
    seq:
      - id: int_len
        type: u1
      - id: int_content
        type:
          switch-on: int_len
          cases:
            1: u1
            2: u2
            4: u4
  lv_str:
    seq:
      - id: str_len
        type: u2
      - id: str_content
        size: str_len
        type: str
        encoding: ascii
  lv_bytes:
    seq:
      - id: bytes_len
        type: u2
      - id: bytes_content
        size: bytes_len
  dict_value:
    seq:
      - id: dict_value_tag
        type: u1
      - id: dict_value_lv
        type:
          switch-on: dict_value_tag
          cases:
            0x01: lv_int
            0x10: lv_str
            0x11: lv_bytes
            0x42: dict_entries
  dict_entry_value:
    seq:
      - id: dict_entry_key_lv
        type: lv_str
      - id: dict_entry_value
        type: dict_value

  dict_entry:
    seq:
      - id: dict_entry_tag
        type: u1
      - id: dict_entry_value
        type:
          switch-on: dict_entry_tag
          cases:
            0x10: dict_entry_value
            0x40: dict_dim
  dict_entries:
    seq:
      - id: dict_entries
        type: dict_entry
        repeat: until
        repeat-until: _.dict_entry_tag == 0x40
      
  dict:
    seq:
      - id: dict_tag
        contents: [0x42]
      - id: dict_entries
        type: dict_entries
  raw_data_list:
    seq:
      - id: data_dict
        type: dict
        repeat: eos

        