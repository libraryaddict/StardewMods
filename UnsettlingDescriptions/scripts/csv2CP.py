import os.path
import json
import csv
from file_info import files_to_convert

non_starters = ["TODO", "UNOBTAINABLE", "NON-INVENTORY", "ALREADY UNSETTLING"]

cp_json_object = {"Changes": []}

# Add Fields patches for each file
for entry in files_to_convert:
    target_name = entry[0]
    desc_index = entry[3]

    # Create patch object
    new_patch = {
        "LogName": f"Edit {target_name} descriptions",
        "Action": "EditData",
        "Target": f"Data/{target_name}",
        "Fields": {}
    }

    # Check if this filled csv file exists
    if not os.path.isfile('csv_filled/Unsettling Item Descriptions - ' + target_name + '.csv'):
        continue

    # Read and enter data from each csv file
    with open('csv_filled/Unsettling Item Descriptions - ' + target_name + '.csv') as csv_file:
        print(target_name)
        reader = csv.DictReader(csv_file)

        for row in reader:

            hasDesc = True
            if row["unsettling_description"] == "":
                hasDesc = False
            for text in non_starters:
                if row["unsettling_description"].startswith(text):
                    hasDesc = False

            if hasDesc:
                new_patch["Fields"][row["key"]] = {desc_index: row["unsettling_description"]}

    # Add patch
    if len(new_patch["Fields"]) > 0:
        cp_json_object["Changes"].append(new_patch)

# Special logic for StringsFromCSFiles Entries patch
target_name = 'StringsFromCSFiles'
if os.path.isfile('csv_filled/Unsettling Item Descriptions - ' + target_name + '.csv'):

    # Create patch object
    new_patch = {
        "LogName": f"Edit {target_name}",
        "Action": "EditData",
        "Target": f"Strings/{target_name}",
        "Entries": {}
    }

    # Read and enter data from the csv file
    with open('csv_filled/Unsettling Item Descriptions - ' + target_name + '.csv') as csv_file:
        print(target_name)
        reader = csv.DictReader(csv_file)

        for row in reader:

            hasDesc = True
            if row["unsettling_description"] == "":
                hasDesc = False
            for text in non_starters:
                if row["unsettling_description"].startswith(text):
                    hasDesc = False

            if hasDesc:
                new_patch["Entries"][row["key"]] = row["unsettling_description"]

    # Add patch
    if len(new_patch["Entries"]) > 0:
        cp_json_object["Changes"].append(new_patch)

# Write to .json file
with open('default.json', 'w') as cp_file:
    json.dump(cp_json_object, cp_file, indent=2)


# with open('default.json', mode='w', newline='') as cp_file:
#     # Open Changes
#     cp_file.writelines([
#         '{',
#         '  "Changes": ['
#     ])
#
#     for entry in files_to_convert:
#         target_name = entry[0]
#         desc_index = entry[3]
#
#         # Check if this filled csv file exists
#         if not os.path.isfile('csv_filled/' + target_name + '.csv'):
#             continue
#
#         # Open patch
#         cp_file.writelines([
#             '    {',
#             f'      "LogName": "Edit {target_name} descriptions",',
#             f'      "Action": "EditData",',
#             f'      "Target": "Data/{target_name}",',
#             '      "Fields": {'
#         ])
#
#         # Read and enter data from each csv file
#         with open('csv_filled/' + target_name + '.csv') as csv_file:
#             reader = csv.DictReader(csv_file)
#
#             line_count = 0
#             for row in reader:
#
#                 hasDesc = True
#                 if row["unsettling_description"] == "":
#                     hasDesc = False
#                 for text in non_starters:
#                     if row["unsettling_description"].startswith(text):
#                         hasDesc = False
#
#                 if line_count != 0 and hasDesc:
#                     cp_file.write(
#                         f'        "{row["key"]}": {{ {desc_index}: "{row["unsettling_description"]}" }}, //{row["name"]}'
#                     )
#                 line_count += 1
#
#         # Close patch
#         cp_file.writelines([
#             '      }',
#             '    },'
#         ])
#
#     # Close Changes
#     cp_file.writelines(['  ]', '}'])
