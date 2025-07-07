#!/usr/bin/env python
# -*- coding: utf-8 -*-

import re
import config
import helper_file as fileHelper
    
def Write(workbook, write_name, write_path, ignore):
    if write_name.endswith('const'):
        _parse_const(workbook, write_path, write_name, ignore)
        return ignore, None
    if write_name.startswith('Language'):
        _parse_language(workbook, write_path)
        return '', None
    if write_name.endswith(config.class_description):
        _parse_description_class(workbook, write_path)
        return '', None
    if write_name.endswith(config.inheritance_description):
        return '', None
    
    typeFullNames = _parse_default(workbook, write_path, write_name, ignore)
    return ignore, typeFullNames

# ----------------------------#
def _parse_const(workbook, write_path, write_name, ignore):
    
    ayRows = list(workbook.sheets[0].Rows)
    contents = []
    for i in range(0, len(ayRows)):
        cells = ayRows[i]
        if cells[0].value is None:
            continue
        triple = [cells[0].value, cells[1].value, cells[2].value, cells[3].value]
        contents.append(triple) #type key des value
        # print('>>row:%s  :%s' % (i+1, triple))

    if ignore != 'ignore_cpp':
        _write_class_cpp(contents, write_path, write_name, 'static ', 'static ', True)

    maps = {}
    for content in contents:
        cellval = _get_cellval(content[3], content[0])
        if cellval is None:
            continue
        maps[content[1]] = cellval

    fileHelper.WriteJson(write_name+'.json', config.json_path + write_path, maps, True)
    
def _parse_language(workbook, write_path):
    worksheet = workbook.sheets[0]
    ayrows = list(worksheet.Rows)
    aycols = list(worksheet.Columns)
    nrows = len(ayrows)  # 行数
    ncols = len(aycols)  # 列数

    descList = ayrows[worksheet.startRow - 3]  # 字段说明
    namesList = ayrows[worksheet.startRow - 2]  # 字段名
    typesList = ayrows[worksheet.startRow - 1]  # 字段类型名称
    
    language_name = []
    language_desc = []
    for index in range(0, len(namesList)):
        sfile = str(namesList[index].value)
        if "None" in sfile or "None" in str(typesList[index].value):
            continue
        content = {}
        language_name.append(sfile)
        language_desc.append(str(descList[index].value))
        if index == 0:
            continue
        for i in range(worksheet.startRow, nrows):
            content[str(ayrows[i][0].value)] = str(ayrows[i][index].value)
        fileHelper.WriteJson(sfile+'.json', config.json_path + write_path + 'Languages/', content, True)
        
    content = []
    for index in range(len(language_name)):
        if index == 0:
            content.append({'index': index, 'name': language_desc[0], 'describe': language_desc[language_name.index(language_desc[0])]})
        else:
            content.append({'index': index, 'name': language_name[index], 'describe': language_desc[index]})
            
    fileHelper.WriteJson('LanguageConfig.json', config.json_path + write_path + 'Languages/', content, True)
    
def _parse_description_class(workbook, write_path):
    if len(workbook.sheets) == 0:
        return
    ayRows = list(workbook.sheets[0].Rows)
    for i in range(0, len(ayRows)):
        cells = ayRows[i]
        classname = cells[0].value
        if classname is None or config.filters in classname:
            continue
        contents = []
        for j in range(1, len(cells)):
            if cells[j].value is None:
                continue
            temp = cells[j].value.split(':')
            if temp[1] in contents:
                raise Exception(u'%s key repetition :%s' % (classname, temp[1]))
            contents.append([temp[0], temp[1], ''])
        if len(contents) > 0:
            _write_class_cpp(contents, write_path, classname)
                
        # print('>>row:%s  :%s' % (i+1, contents))

def _parse_default(workbook, write_path, write_name, ignore, cls_name=''):
    if cls_name == '':
        cls_name = write_name
        
    infos = []
    typeFullNames = []
    for i in range(len(workbook.sheets)):
        worksheet = workbook.sheets[i]
        info = _worksheet_to_json(worksheet, cls_name, worksheet.sTitle)
        if 'group#' in worksheet.sTitle:
            infos.append(info)
        elif 'out#' in worksheet.sTitle:
            if i == 0:
                typeFullNames.append(info['typeFullName'])
                _worksheet_write(info['jsonname'], write_path, cls_name, info['contents'], ignore, [info])
            else:
                _worksheet_write(info['jsonname'], write_path, info['classname'], info['contents'], 'ignore_cpp', [info])
        else:
            typeFullNames.append(info['typeFullName'])
            _worksheet_write(info['jsonname'], write_path, info['classname'], info['contents'], ignore, [info])
        
    count = len(infos)
    if count == 0:
        return typeFullNames
    
    typeFullNames.append(cls_name)
    contents = {}
    for info in infos:
        contents[info['fieldname']] = info['contents']

    _worksheet_write(info['jsonname'], write_path, cls_name, contents, ignore, infos)
    return typeFullNames

def _worksheet_write(json_name, write_path, cls_name, contents, ignore, infos):
    if ignore != 'ignore_cpp':
        _worksheet_to_cpp(write_path, cls_name, infos)
        
    fileHelper.WriteJson(json_name+'.json', config.json_path + write_path, contents, True) #ayclass[0]['format'])
    

# ----------------------------#
def _get_class_cpp_contents(cls_cfg):
    contents = []
    namesList = cls_cfg['namesList']
    typesList = cls_cfg['typesList']
    descList = cls_cfg['descList']
    for i in range(len(namesList)):
        if typesList[i].value is None:
            continue
        value_type = typesList[i].value#.split('&')[0]
        if namesList[i].value.startswith('_key_'):
            names = namesList[i].value.split(':')
            if len(names) == 2:
                contents.append([value_type, names[1], ''])
        else:
            contents.append([value_type, namesList[i].value, descList[i].value])
    return contents

def _get_class_cpp_info(contents, clsname, cppEndName, clshead='', paramhead='', const_body='', typename = None, writepath=''):
    sbody = 'public %spartial class %s%s {\n' % (clshead, clsname, cppEndName)
    clsParses = []
    for content in contents:
        tempname = content[0]
        if '@' in tempname:
            tempname = tempname.replace('@', '')
            clsParses.append([tempname, content[1]])
            
        sbody += '\tpublic %s%s %s' % (paramhead, tempname.split('&')[0], content[1])
        sbody += ';\t//%s\n' % (content[2])

    if len(clsParses) > 0:
        sbody += '\t[Newtonsoft.Json.JsonProperty] internal string[] __temporary;\n'

    sbody += const_body
    sbody += '}\n'
    
    if typename != None:
        sbody += _add_class_config_deserialize(clsname, typename, writepath, clsParses)
    return sbody

def _add_class_config_deserialize(clsname, typename, writepath, clsParses):
    sbody = '\npublic static class %sLoader {\n' % (clsname)
    
    isDict = 'Dictionary' in typename
    classParse = _add_class_config_classParse(clsParses, isDict)
    keyvalue = typename
    if isDict:
        keyvalue = typename[typename.find('<') + 1:typename.rfind('>')]
    
    sbody += '''
    public static %s Deserialize(string file = null) {
        file ??= "%s%s";
        var configs = ConfigManager.LoadTableJson<%s>(file);%s
        return configs;
    }
''' % (typename, config.replenish_path_start + writepath, clsname, keyvalue, classParse)
        
    sbody += '}\n'
    return sbody
    
def _add_class_config_classParse(clsParses, isDict):
    if len(clsParses) == 0:
        return ''
    
    sbody = ''
    for v in clsParses:
        # print('_add_class_config_classParse', v)
        typename = v[0]
        fieldname = v[1]
        if 'List' in typename:
            clsname = typename[typename.find('<') + 1:typename.rfind('>')]
            sbody += '''
            {
                var values = cfg.__temporary[++counter].Split('|');
                cfg.%s = new %s();
			    foreach (var value in values)
					cfg.%s.Add(%s.Parse(value));
            }
''' % (fieldname, typename, fieldname, clsname)
        # elif 'Dictionary' in typename:
        #     sparam = typename[typename.find('<') + 1:typename.rfind('>')].split(',')
        #     key = sparam[0]
        #     clsname = sparam[1]
        else:
            sbody += '\n\t\t\tcfg.%s = %s.Parse(cfg.__temporary[++counter]);' % (fieldname, typename)
        
    return '''
	    foreach (var cfg in %s)
	    {
		    var counter = -1;
%s
            cfg.__temporary = null;
	    }
''' % (isDict and 'configs.Values' or 'configs', sbody)
    
def _write_class_cpp(contents, write_path, clsname, clshead='', paramhead='', bconst=False, complex_body='', typename = None):
    sbody = ''
    sbody += 'using System;\n' 
    sbody += 'using System.Collections.Generic;\n' 
    sbody += 'using UnityEngine;\n' 
    const_body = ''
    if bconst:
        const_body = '''
    public static void Deserialize() {
	    ConfigManager.ParseConstJson(typeof(%s), "%s%s");
    }
''' % (clsname, config.replenish_path_start + write_path, clsname)
        
    cppEndName = config.cpp_end_name
    if bconst == True:
        typename = None
        cppEndName = ''
    
    sbody += complex_body
    sbody += _get_class_cpp_info(contents, clsname, cppEndName, clshead, paramhead, const_body, typename=typename, writepath=write_path)
    fileHelper.WriteFile(clsname+cppEndName+'.cs', config.cls_path + write_path, sbody)
    # ----------------------------#    
def _worksheet_to_json(worksheet, cls_name, field_name):
    ayrows = list(worksheet.Rows)
    aycols = list(worksheet.Columns)
    nrows = len(ayrows)  # 行数
    ncols = len(aycols)  # 列数

    descList = ayrows[worksheet.startRow - 3]  # 字段说明
    namesList = ayrows[worksheet.startRow - 2]  # 字段名
    typesList = ayrows[worksheet.startRow - 1]  # 字段类型名称
    
    info = {
        # 'format': '#' in field_name,
        'fieldname': field_name[field_name.find('#') + 1:],
        'descList': descList,
        'namesList': namesList,
        'typesList': typesList,
        'classname': cls_name,
        'jsonname': cls_name
    }
    if 'group#' in field_name: 
        if info['fieldname'][0] == '_':
            info['classname'] = cls_name + info['fieldname']
        else:
            info['classname'] = info['fieldname']
    if 'out#' in field_name:
        if info['fieldname'][0] == '_':
            info['jsonname'] = cls_name + info['fieldname']
        else:
            info['jsonname'] = info['fieldname']
    
    # print 'descList', descList
    # print 'namesList', namesList
    # print 'typesList', typesList
    if namesList[0].value.startswith('_key_'):
        contents, typeFullName = _worksheet_to_json_map(nrows, ncols, ayrows, typesList, namesList, info['classname'], worksheet.startRow)
    else:
        contents, typeFullName = _worksheet_to_json_array(nrows, ncols, ayrows, typesList, namesList, info['classname'], worksheet.startRow)
        
    info['contents'] = contents
    info['typeFullName'] = typeFullName
    return info
    
def _worksheet_to_json_array(nrows, ncols, ayrows, typesList, namesList, cls_name, startRowNum):
    contentList = []
    for rownum in range(startRowNum, nrows):  # 内容从第5行开始
        rowval = ayrows[rownum]
        if rowval[0].value is None:
            continue
        rowContentTable = _worksheet_to_json_one(0, ncols, rowval, typesList, namesList)
        contentList.append(rowContentTable)
        
    typeFullName = 'List<%s%s>' % (cls_name, config.cpp_end_name)
    return contentList, typeFullName

def _worksheet_to_json_map(nrows, ncols, ayrows, typesList, namesList, cls_name, startRowNum):
    contentList = {}
    typeOne = typesList[0].value.split('&')
    isArray = len(typeOne) > 1
    
    for rownum in range(startRowNum, nrows):  # 内容从第5行开始
        rowval = ayrows[rownum]
        key = rowval[0].value
        if key is None:
            continue
        
        # print('>>>_worksheet_to_json_map rownum: ' , rownum)
        rowContentTable = _worksheet_to_json_one(1, ncols, rowval, typesList, namesList)
        names = namesList[0].value.split(':')
        if len(names) == 2:
            cellval = _get_cellval(rowval[0].value, typeOne[0])
            if cellval is not None:
                rowContentTable[names[1]] = cellval
        
        if isArray:
            if key not in contentList:
                contentList[key] = []
            contentList[key].append(rowContentTable)
        else:
            if key in contentList:
                raise Exception(u'[=======error=======]: !!!!!!!key [%s] repetition!!!!!!!' % key)
            else: 
                contentList[key] = rowContentTable
    
    typeFullName = 'Dictionary<%s, %s%s>' % (typeOne[0], cls_name, config.cpp_end_name)
    if isArray:
        if 'List' in typeOne[1]:
            typeFullName = 'Dictionary<%s, List<%s%s>>' % (typeOne[0], cls_name, config.cpp_end_name)
        else:
            typeFullName = 'Dictionary<%s, %s%s[]>' % (typeOne[0], cls_name, config.cpp_end_name)
    return contentList, typeFullName    

def _worksheet_to_json_one(nstarti, ncols, rowval, typesList, namesList):
    rowContentTable = {}
    clsInfoTemporary = []
    for colnum in range(nstarti, ncols):
        # 第rownum行 第colnum列的值
        cellval = None
        typename = typesList[colnum].value
        if typename != None and '@' in typename:
            clsInfoTemporary.append([str(rowval[colnum].value), typename.replace('@', ''), namesList[colnum].value])
        else:
            cellval = _get_cellval(rowval[colnum].value, typename)
        if cellval is None:
            continue
        rowContentTable[namesList[colnum].value] = cellval
    if len(clsInfoTemporary) > 0:
        rowContentTable['__temporary'] = []
        for v in clsInfoTemporary:
            rowContentTable['__temporary'].append(v[0])
        # rowContentTable['__temporary__'] = clsInfoTemporary
    return rowContentTable
    
# ----------------------------#
def _worksheet_to_cpp(write_path, cls_name, infos):
    count = len(infos)
    if count == 0:
        raise Exception('excel_parse_cpp ayClass 0?')
    
    if count == 1:
        # print('_worksheet_to_cpp', fileHelper.json_dumps(infos[0]['contents'], False))
        contents = _get_class_cpp_contents(infos[0])
        _write_class_cpp(contents, write_path, cls_name, typename=infos[0]['typeFullName'])
        return
    
    complex_body = ''
    contents = []
    for info in infos:
        contents.append([info['typeFullName'], info['fieldname'], ''])
        complex_body += _get_class_cpp_info(_get_class_cpp_contents(info), info['classname'], config.cpp_end_name)
    _write_class_cpp(contents, write_path, cls_name, complex_body=complex_body, typename = cls_name + config.cpp_end_name)

# ----------------------------#
def _get_cellval(cellval, types):
    # print('_get_cellval', cellval, types)
    if types == "string" and cellval is None:
        cellval = ''
    elif types is None or cellval is None:
        return None
    
    if 'Dictionary' in types:
        value_type = types.split('&')
        stemp = value_type[0][value_type[0].find('<') + 1:value_type[0].rfind('>')].split(',', 1)
        skey = stemp[0].strip()
        stype = stemp[1].strip()
        splits = _get_split_sign(['|', ':', ',', '-'], value_type)
        
        cellval = str(cellval).replace('\n', '').strip()
        cellval = cellval.split(splits[0])
        dictionary = {}
        for sparam in cellval:
            param = sparam.split(splits[1])
            for i in range(len(param)):
                param[i] = param[i].replace('\n', '').strip()
            if types.startswith('List') or '[]' in stype:
                dictionary[param[0]] = _get_list(param[1].split(splits[2]), types, splits[3])
            else:
                dictionary[param[0]] = _get_field(param[1], stype)
        
        cellval = dictionary
        
    elif '[][]' in types or '[,]' in types:
        value_type = types.split('&')
        splits = _get_split_sign(['|', ',', '-'], value_type)

        tempval = str(cellval).replace('\n', '').strip()
        cellval = tempval
        cellval = cellval.split(splits[0])
        vals = []
        for val in cellval:
            vals.append(_get_list(val.split(splits[1]), types, splits[2]))
        cellval = vals 
        if '[,]' in types: #检查数据
            length = len(cellval[0])
            # print('======:', types, length, tempval)
            for v in cellval:
                if length != len(v):
                    print(u'[=======error=======]: !!!!!!!data error!!!!!!!')
                    raise Exception(tempval)
        
    elif types.startswith('List') or '[]' in types:
        value_type = types.split('&')
        splits = _get_split_sign(['|', ','], value_type)
        
        cellval = str(cellval).replace('\n', '').strip()
        cellval = _get_list(cellval.split(splits[0]), types, splits[1])
        
    else:
        cellval = _get_field(cellval, types)
        
    return cellval

def _get_field(cellval, types):
            # int默认读出是float，需转化一下
    if types == 'int':
        cellval = int(cellval)
    elif types == 'string':
        cellval = str(cellval)
    elif types == 'bool':
        cellval = str(cellval).lower() in ('yes','true','1')
    elif types == 'float':
        cellval = float(re.findall(r"[-+]?\d*\.\d+|\d+", str(cellval))[0])
    elif types == 'Vector3':
        cellval = list(map(float,cellval.split(',')))
        cellval = {'x': cellval[0], 'y': cellval[1], 'z': cellval[2]}
    elif types == 'Vector2':
        cellval = list(map(float,cellval.split(',')))
        cellval = {'x': cellval[0], 'y': cellval[1]}
    elif '@' in types:
        cellval = str(cellval)
    else: #Class
        value_type = types.split('&')
        splits = _get_split_sign(['|'], value_type)
        cellval = _get_class(cellval.split(splits[0]), value_type[0])
        
    return cellval

def _get_list(cellval, types, sign):
    if '<int[]>' in types:
        array = []
        for v in list(cellval):
            array.append(list(map(int,v.split(sign))))
        cellval = array
    elif '<float[]>' in types:
        array = []
        for v in list(cellval):
            array.append(list(map(float,v.split(sign))))
        cellval = array
    elif '<string[]>' in types:
        array = []
        for v in list(cellval):
            array.append(list(v.split(sign)))
        cellval = array
    elif 'int' in types:
        cellval = list(map(int,cellval))
    elif 'float' in types:
        cellval = list(map(float,cellval))
    elif 'string' in types:
        cellval = list(cellval)
    elif 'Vector3' in types:
        array = []
        for v in list(cellval):
            one = list(map(float,v.split(sign)))
            one = {'x': one[0], 'y': one[1], 'z': one[2]}
            array.append(one)
        cellval = array
    elif 'Vector2' in types:
        array = []
        for v in list(cellval):
            one = list(map(float,v.split(sign)))
            one = {'x': one[0], 'y': one[1]}
            array.append(one)
        cellval = array
    else:
        classname = None
        if types.startswith('List'):
            classname = types[types.find('<') + 1:types.rfind('>')].strip()
        else:
            classname = types.split('[')[0].strip()
        vals = []
        for val in cellval:
            vals.append(_get_class(val.split(sign), classname))
        cellval = vals 
        
    return cellval

def _get_class(cellval, classname):
    # print('============getclass', cellval, classname)
    classtable = fileHelper.GetClassDescriptionTable(classname)
    # print('classtable', classtable)
    if cellval[0][0] == '#':
        inheritancetable = fileHelper.GetInheritanceDescriptionTable(classname)
        # print('inheritancetable', inheritancetable)

    obj = {}
    index = 0
    for i in range(len(cellval)):
        cell = cellval[i]
        if i == 0 and cell[0] == '#':
            vals = int(cell[1:])
            obj['$type'] = '%s, Assembly-CSharp' % (inheritancetable[vals - 1])
            classtable = fileHelper.GetClassDescriptionTable(inheritancetable[vals - 1], False)
            if classtable == None:
                classtable = fileHelper.GetClassDescriptionTable(classname)
            # print('>>>change', i, classtable)
            continue


        vals = cell.split('#')
        if len(vals) > 1:
            one = None
            for tab in classtable:
                if tab[1] == vals[0]:
                    one = tab
                    break
            if one is None:
                raise Exception(u'%s field %s nonentity' % (classname, cell))
            obj[one[1]] = _get_cellval(vals[1], one[0])
            continue
        
        obj[classtable[index][1]] = _get_cellval(vals[0], classtable[index][0])
        index += 1
    return obj

def _get_split_sign(splits, value_type):
    for i in range(1, len(value_type)):
        splits[i - 1] = value_type[i]
    return splits
    